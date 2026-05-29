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
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
					};
				});

			return services;
		}
	}
}
