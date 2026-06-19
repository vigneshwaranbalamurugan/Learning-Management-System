using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.RateLimiting;
using System.Security.Claims;

namespace LMSApi.API.Extensions
{
    public static class RateLimiterExtensions
    {

        private readonly IConfiguration _configuration;

        public RateLimiterExtensions(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    var errorResponse = new
                    {
                        StatusCode = StatusCodes.Status429TooManyRequests,
                        Message = "Too many requests. Please try again later.",
                        TraceId = context.HttpContext.TraceIdentifier,
                        Success = false
                    };
                    await context.HttpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken: token);
                };

                string GetClientIp(HttpContext httpContext)
                {
                    if (httpContext.Request.Headers.TryGetValue("CF-Connecting-IP", out var cfIp))
                    {
                        return cfIp.ToString();
                    }
                    if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
                    {
                        var ip = forwardedFor.ToString().Split(',').FirstOrDefault()?.Trim();
                        if (!string.IsNullOrEmpty(ip)) return ip;
                    }
                    return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                }

                // 1. Login - 5 req/min per IP
                options.AddPolicy("Login", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetClientIp(context),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = _configuration.GetValue<int>("RateLimiting:LoginPermitLimit", 5),
                            Window = TimeSpan.FromMinutes(1)
                        }));

                // 2. Forgot Password - 3 req/15 min per Email + IP
                options.AddPolicy("ForgotPassword", context =>
                {
                    var ip = GetClientIp(context);
                    var email = context.Items.TryGetValue("ForgotPasswordEmail", out var emailVal) ? emailVal?.ToString() : string.Empty;
                    var key = $"ForgotPassword_{email}_{ip}";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: key,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = _configuration.GetValue<int>("RateLimiting:ForgotPasswordPermitLimit", 3),
                            Window = TimeSpan.FromMinutes(15)
                        });
                });

                // 3. OTP Send - 3 req/5 min
                options.AddPolicy("OtpSend", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetClientIp(context),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = _configuration.GetValue<int>("RateLimiting:OtpSendPermitLimit", 3),
                            Window = TimeSpan.FromMinutes(5)
                        }));

                // 4. OTP Verify - 10 req/5 min
                options.AddPolicy("OtpVerify", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetClientIp(context),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = _configuration.GetValue<int>("RateLimiting:OtpVerifyPermitLimit", 10),
                            Window = TimeSpan.FromMinutes(5)
                        }));

                // 5. Register - 5 req/hour per IP
                options.AddPolicy("Register", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetClientIp(context),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = _configuration.GetValue<int>("RateLimiting:RegisterPermitLimit", 5),
                            Window = TimeSpan.FromHours(1)
                        }));

                // 6. Public Course Listing - 100 req/min
                options.AddPolicy("PublicCourseListing", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetClientIp(context),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = _configuration.GetValue<int>("RateLimiting:PublicCourseListingPermitLimit", 100),
                            Window = TimeSpan.FromMinutes(1)
                        }));

                // 7. Search Courses - 60 req/min
                options.AddPolicy("SearchCourses", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetClientIp(context),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = _configuration.GetValue<int>("RateLimiting:SearchCoursesPermitLimit", 60),
                            Window = TimeSpan.FromMinutes(1)
                        }));

                // 8. Enroll Course - 10 req/min
                options.AddPolicy("EnrollCourse", context =>
                {
                    var userId = context.User.Identity?.IsAuthenticated == true ? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;
                    var partitionKey = userId != null ? $"User_{userId}" : $"IP_{GetClientIp(context)}";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: partitionKey,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = _configuration.GetValue<int>("RateLimiting:EnrollCoursePermitLimit", 10),
                            Window = TimeSpan.FromMinutes(1)
                        });
                });

                // 9. Payment Initialization - 3 req/min
                options.AddPolicy("PaymentInitialization", context =>
                {
                    var userId = context.User.Identity?.IsAuthenticated == true ? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;
                    var partitionKey = userId != null ? $"User_{userId}" : $"IP_{GetClientIp(context)}";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: partitionKey,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = _configuration.GetValue<int>("RateLimiting:PaymentInitializationPermitLimit", 3),
                            Window = TimeSpan.FromMinutes(1)
                        });
                });

                // 10. Quiz Submit - 20 req/min
                options.AddPolicy("QuizSubmit", context =>
                {
                    var userId = context.User.Identity?.IsAuthenticated == true ? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;
                    var partitionKey = userId != null ? $"User_{userId}" : $"IP_{GetClientIp(context)}";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: partitionKey,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = _configuration.GetValue<int>("RateLimiting:QuizSubmitPermitLimit", 20),
                            Window = TimeSpan.FromMinutes(1)
                        });
                });

                // 11. Assignment Submit - 10 req/min
                options.AddPolicy("AssignmentSubmit", context =>
                {
                    var userId = context.User.Identity?.IsAuthenticated == true ? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;
                    var partitionKey = userId != null ? $"User_{userId}" : $"IP_{GetClientIp(context)}";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: partitionKey,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = _configuration.GetValue<int>("RateLimiting:AssignmentSubmitPermitLimit", 10),
                            Window = TimeSpan.FromMinutes(1)
                        });
                });

                // 12. File Upload - 20 req/hour
                options.AddPolicy("FileUpload", context =>
                {
                    var userId = context.User.Identity?.IsAuthenticated == true ? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;
                    var partitionKey = userId != null ? $"User_{userId}" : $"IP_{GetClientIp(context)}";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: partitionKey,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = _configuration.GetValue<int>("RateLimiting:FileUploadPermitLimit", 20),
                            Window = TimeSpan.FromHours(1)
                        });
                });

                // 13. Certificate Download - 30 req/min
                options.AddPolicy("CertificateDownload", context =>
                {
                    var userId = context.User.Identity?.IsAuthenticated == true ? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;
                    var partitionKey = userId != null ? $"User_{userId}" : $"IP_{GetClientIp(context)}";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: partitionKey,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = _configuration.GetValue<int>("RateLimiting:CertificateDownloadPermitLimit", 30),
                            Window = TimeSpan.FromMinutes(1)
                        });
                });

                // 14. Notification APIs - 60 req/min
                options.AddPolicy("NotificationApis", context =>
                {
                    var userId = context.User.Identity?.IsAuthenticated == true ? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;
                    var partitionKey = userId != null ? $"User_{userId}" : $"IP_{GetClientIp(context)}";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: partitionKey,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = _configuration.GetValue<int>("RateLimiting:NotificationApisPermitLimit", 60),
                            Window = TimeSpan.FromMinutes(1)
                        });
                });

                // 15. Admin APIs - 200 req/min
                options.AddPolicy("AdminApis", context =>
                {
                    var userId = context.User.Identity?.IsAuthenticated == true ? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;
                    var partitionKey = userId != null ? $"User_{userId}" : $"IP_{GetClientIp(context)}";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: partitionKey,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = _configuration.GetValue<int>("RateLimiting:AdminApisPermitLimit", 200),
                            Window = TimeSpan.FromMinutes(1)
                        });
                });

                // 16. SignalR Hub Connect(Notification) - 20 connections/min
                options.AddPolicy("SignalRHubConnectNotification", context =>
                {
                    var userId = context.User.Identity?.IsAuthenticated == true ? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;
                    var partitionKey = userId != null ? $"User_{userId}" : $"IP_{GetClientIp(context)}";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: partitionKey,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = _configuration.GetValue<int>("RateLimiting:SignalRHubConnectNotificationPermitLimit", 20),
                            Window = TimeSpan.FromMinutes(1)
                        });
                });

                // 17. SignalR Hub Connect(VideoProgress) - 20 connections/min
                options.AddPolicy("SignalRHubConnectVideoProgress", context =>
                {
                    var userId = context.User.Identity?.IsAuthenticated == true ? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value : null;
                    var partitionKey = userId != null ? $"User_{userId}" : $"IP_{GetClientIp(context)}";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: partitionKey,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = _configuration.GetValue<int>("RateLimiting:SignalRHubConnectVideoProgressPermitLimit", 20),
                            Window = TimeSpan.FromMinutes(1)
                        });
                });
            });

            return services;
        }
    }
}
