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
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors
                            .Select(e => e.ErrorMessage)
                            .ToArray()
                    );

                var response = new
                {
                    success = false,
                    message = "Validation failed",
                    errors = errors
                };

                return new BadRequestObjectResult(response);
            };
        });

        return services;
    }
}