using Microsoft.AspNetCore.Mvc;

namespace LMSApi.API.Extensions;

public static class ApiBehaviorExtensions
{
    public static IServiceCollection AddCustomValidation(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors
                            .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                                ? "Invalid value."
                                : e.ErrorMessage)
                            .ToArray());

                var traceId = context.HttpContext.TraceIdentifier;

                var response = new
                {
                    success    = false,
                    statusCode = 400,
                    message    = "Validation failed. Please check the errors and try again.",
                    traceId,
                    errors
                };

                return new BadRequestObjectResult(response);
            };
        });

        return services;
    }
}