using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace LMSApi.API.Middlewares
{
    public class ForgotPasswordEmailExtractorMiddleware
    {
        private readonly RequestDelegate _next;

        public ForgotPasswordEmailExtractorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.Value != null &&
                context.Request.Path.Value.Contains("/auth/forgot-password", StringComparison.OrdinalIgnoreCase) &&
                HttpMethods.IsPost(context.Request.Method))
            {
                context.Request.EnableBuffering();

                using (var reader = new StreamReader(context.Request.Body, leaveOpen: true))
                {
                    var bodyText = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0; // Reset body stream position for model binding

                    if (!string.IsNullOrEmpty(bodyText))
                    {
                        try
                        {
                            using (var jsonDoc = JsonDocument.Parse(bodyText))
                            {
                                if (jsonDoc.RootElement.TryGetProperty("email", out var emailProp))
                                {
                                    var email = emailProp.GetString()?.Trim().ToLowerInvariant();
                                    if (!string.IsNullOrEmpty(email))
                                    {
                                        context.Items["ForgotPasswordEmail"] = email;
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // Catch JSON parsing errors gracefully and let binding handle it later
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
