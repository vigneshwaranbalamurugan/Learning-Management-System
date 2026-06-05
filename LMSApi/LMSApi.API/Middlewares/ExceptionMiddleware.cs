using System.Data.Common;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace LMSApi.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }

            // ── Client / domain errors (4xx) ────────────────────────────────

            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access. Path={Path}", context.Request.Path);
                await WriteErrorAsync(context, HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Resource not found. Path={Path}", context.Request.Path);
                await WriteErrorAsync(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation. Path={Path}", context.Request.Path);
                await WriteErrorAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (ArgumentException ex)               // ArgumentNullException is a subtype
            {
                _logger.LogWarning(ex, "Bad argument. Path={Path}", context.Request.Path);
                await WriteErrorAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }
            catch (FormatException ex)
            {
                _logger.LogWarning(ex, "Format error. Path={Path}", context.Request.Path);
                await WriteErrorAsync(context, HttpStatusCode.BadRequest, "One or more values are in an incorrect format.");
            }
            catch (NotSupportedException ex)
            {
                _logger.LogWarning(ex, "Not supported. Path={Path}", context.Request.Path);
                await WriteErrorAsync(context, HttpStatusCode.BadRequest, ex.Message);
            }

            // ── Client disconnect — log info, do not write a response ───────
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation("Request cancelled by client. Path={Path}", context.Request.Path);
                // Response is already abandoned; nothing to write.
            }

            // ── Database / infrastructure errors (5xx) ──────────────────────
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict. Path={Path}", context.Request.Path);
                await WriteErrorAsync(
                    context, HttpStatusCode.Conflict,
                    "The resource was modified by another request. Please reload and try again.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error. Path={Path} InnerMessage={Inner}",
                    context.Request.Path, ex.InnerException?.Message);

                // Hide raw SQL/constraint details from the client in production
                var dbUpdateMsg = _env.IsDevelopment()
                    ? $"Database error: {ex.InnerException?.Message ?? ex.Message}"
                    : "A database error occurred. Please try again later.";

                await WriteErrorAsync(context, HttpStatusCode.InternalServerError, dbUpdateMsg);
            }

            // ── Npgsql-level errors: connection failures, timeouts, constraint violations ──
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "PostgreSQL error. Path={Path} SqlState={SqlState} InnerMessage={Inner}",
                    context.Request.Path, ex.SqlState, ex.InnerException?.Message ?? ex.Message);

                await WriteErrorAsync(
                    context, HttpStatusCode.InternalServerError,
                    "A database error occurred. Please try again later.");
            }

            // ── Generic ADO.NET base — catches any other provider-level DB error ──
            catch (DbException ex)
            {
                _logger.LogError(ex, "Database exception. Path={Path} InnerMessage={Inner}",
                    context.Request.Path, ex.InnerException?.Message ?? ex.Message);

                await WriteErrorAsync(
                    context, HttpStatusCode.InternalServerError,
                    "A database error occurred. Please try again later.");
            }

            // ── Catch-all: truly unexpected server errors ────────────────────
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception. Path={Path} Method={Method}",
                    context.Request.Path, context.Request.Method);

                var msg = _env.IsDevelopment()
                    ? ex.Message
                    : "An unexpected error occurred. Please try again later.";

                await WriteErrorAsync(context, HttpStatusCode.InternalServerError, msg);
            }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private static async Task WriteErrorAsync(
            HttpContext context, HttpStatusCode statusCode, string message)
        {
            // If response streaming has already started (headers sent) we cannot write.
            if (context.Response.HasStarted) return;

            context.Response.ContentType = "application/json";
            context.Response.StatusCode  = (int)statusCode;

            var body = new ErrorResponse(
                StatusCode: (int)statusCode,
                Message:    message,
                TraceId:    context.TraceIdentifier);

            var json = JsonSerializer.Serialize(body, _jsonOptions);
            await context.Response.WriteAsync(json);
        }

        /// <summary>Consistent error envelope — matches the validation error shape in ApiBehaviourExtensions.</summary>
        private sealed record ErrorResponse(
            int    StatusCode,
            string Message,
            string TraceId,
            bool   Success = false);
    }
}