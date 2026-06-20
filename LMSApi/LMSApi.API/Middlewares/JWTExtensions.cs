using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace LMSApi.API.Extensions
{
	public static class JWTConfiguration
	{
		public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
		{
			var jwtKey = configuration["Jwt:Key"] ?? string.Empty;
			if (string.IsNullOrEmpty(jwtKey))
			{
				return services;
			}

			services.AddAuthentication(options =>
				{
					options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
					options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				})
				.AddJwtBearer(options =>
				{
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = true,
						ValidateAudience = true,
						ValidateIssuerSigningKey = true,
						ValidateLifetime = true,
						ValidIssuer = configuration["Jwt:Issuer"] ?? "LMSApi",
						ValidAudience = configuration["Jwt:Audience"] ?? "LMSApiUsers",
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
						ClockSkew = TimeSpan.Zero
					};

					options.Events = new JwtBearerEvents
					{
						OnMessageReceived = context =>
						{
							var cookieToken = context.Request.Cookies["access_token"];
							if (!string.IsNullOrEmpty(cookieToken))
							{
								context.Token = cookieToken;
							}
							else
							{
								var accessToken = context.Request.Query["access_token"];
								var path = context.HttpContext.Request.Path;
								if (!string.IsNullOrEmpty(accessToken) &&
									path.StartsWithSegments("/hubs/notification"))
								{
									context.Token = accessToken;
								}
							}
							return Task.CompletedTask;
						},
						OnAuthenticationFailed = context =>
						{
							context.Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized;
							context.Response.ContentType = "application/json";

							string message = "Authentication failed.";
							if (context.Exception is SecurityTokenExpiredException)
							{
								message = "Token has expired. Please log in again.";
							}
							else if (context.Exception is SecurityTokenInvalidSignatureException)
							{
								message = "Invalid token signature.";
							}
							else
							{
								message = $"Token validation failed: {context.Exception.Message}";
							}

							var errorResponse = new
							{
								success = false,
								statusCode = Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized,
								message = message,
								traceId = context.HttpContext.TraceIdentifier
							};

							var json = System.Text.Json.JsonSerializer.Serialize(errorResponse);
							return context.Response.WriteAsync(json);
						},
						OnForbidden = context =>
						{
							context.Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden;
							context.Response.ContentType = "application/json";

							var errorResponse = new
							{
								success = false,
								statusCode = Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden,
								message = "You do not have permission to access this resource.",
								traceId = context.HttpContext.TraceIdentifier
							};

							var json = System.Text.Json.JsonSerializer.Serialize(errorResponse);
							return context.Response.WriteAsync(json);
						},
						OnChallenge = context =>
						{
							context.HandleResponse();

							if (!context.Response.HasStarted)
							{
								context.Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized;
								context.Response.ContentType = "application/json";

								var authHeader = context.Request.Headers["Authorization"].ToString();
								string message;

								if (string.IsNullOrWhiteSpace(authHeader))
								{
									message = "Authorization token is missing. Please log in to access this resource.";
								}
								else if (context.AuthenticateFailure is SecurityTokenExpiredException)
								{
									message = "Token has expired. Please log in again.";
								}
								else if (context.AuthenticateFailure != null)
								{
									message = $"Invalid token: {context.AuthenticateFailure.Message}";
								}
								else
								{
									message = "Unauthorized access. Please provide a valid authentication token.";
								}

								var errorResponse = new
								{
									success = false,
									statusCode = Microsoft.AspNetCore.Http.StatusCodes.Status401Unauthorized,
									message = message,
									traceId = context.HttpContext.TraceIdentifier
								};

								var json = System.Text.Json.JsonSerializer.Serialize(errorResponse);
								return context.Response.WriteAsync(json);
							}

							return Task.CompletedTask;
						}
					};
				});

			return services;
		}
	}
}
