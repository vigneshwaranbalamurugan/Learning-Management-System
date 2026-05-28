using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace LMSApi.API.Middlewares
{
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt.");
            await HandleExceptionAsync(httpContext, HttpStatusCode.Unauthorized, "Unauthorized access. Please provide valid credentials.");
        }catch(KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found.");
            await HandleExceptionAsync(httpContext, HttpStatusCode.NotFound, "The requested resource was not found.");
        }catch(InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation attempted.");
            await HandleExceptionAsync(httpContext, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(httpContext, HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.");
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, HttpStatusCode httpStatusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)httpStatusCode;
        var response= new
        {
            StatusCode = context.Response.StatusCode,
            Message = message
        };
        var jsonResponse = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(jsonResponse);
    }
}
}