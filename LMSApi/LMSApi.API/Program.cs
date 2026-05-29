using LMSApi.DALLibrary.Contexts;
using LMSApi.API.Extensions;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.DALLibrary.Repositories;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Services;
using LMSApi.BALLibrary.Services.Authentication;
using LMSApi.API.Middlewares;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddCustomValidation();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Database
var connection = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connection))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured. Set it in appsettings.json or environment variables.");
}
builder.Services.AddDbContext<LMSDbContext>(opts => opts.UseNpgsql(connection, npgsqlOptions =>
{
    npgsqlOptions.MigrationsAssembly(typeof(LMSDbContext).Assembly.GetName().Name);
}));
#endregion

#region Dependency Injection for Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
#endregion

#region Dependency Injection for Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationHandler, EmailNotificationHandler>();
builder.Services.AddScoped<INotificationHandler, SmsNotificationHandler>();
builder.Services.AddSingleton<ITokenService, TokenService>();
#endregion

// JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? string.Empty;
if (!string.IsNullOrEmpty(jwtKey))
{
    builder.Services.AddAuthentication(options =>
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
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "LMSApi",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "LMSApiUsers",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
    builder.Services.AddAuthorization();
}

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);

    // If version not specified → use default
    options.AssumeDefaultVersionWhenUnspecified = true;

    // Adds supported/deprecated versions in response headers
    options.ReportApiVersions = true;

    // Read version from URL
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";

    options.SubstituteApiVersionInUrl = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    
}
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
