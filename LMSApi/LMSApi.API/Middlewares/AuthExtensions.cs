using Microsoft.Extensions.DependencyInjection;

namespace LMSApi.API.Extensions
{
	public static class Authorization
	{
		public static IServiceCollection AddRoleAuthorization(this IServiceCollection services)
		{
			services.AddAuthorization(options =>
			{
				options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
				options.AddPolicy("InstructorOnly", policy => policy.RequireRole("Instructor"));
				options.AddPolicy("LearnerOnly", policy => policy.RequireRole("Learner"));
			});

			return services;
		}
	}
}
