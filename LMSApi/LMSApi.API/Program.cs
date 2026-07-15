using LMSApi.DALLibrary.Contexts;
using LMSApi.API.Extensions;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.DALLibrary.Repositories;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Services;
using LMSApi.BALLibrary.Services.Upload;
using LMSApi.BALLibrary.Interfaces.Quizzes;
using LMSApi.BALLibrary.Services.Quizzes;
using LMSApi.BALLibrary.Services.Notification;
using LMSApi.BALLibrary.Mappers;
using LMSApi.BALLibrary.Utils;
using LMSApi.API.Middlewares;
using LMSApi.API.Handlers;
using LMSApi.BALLibrary.Interfaces.Users;
using Azure.Storage.Blobs;
using LMSApi.ModelLibrary.Models;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.PostgreSql;
using LMSApi.API.Filters;
using LMSApi.API.Hubs;
using LMSApi.API.Services;
using LMSApi.API.Handlers;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Security.Claims;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web host");
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Required to resolve PostgreSQL EF Core timezone mismatch errors when storing DateTime.Now
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddSignalR();
    builder.Services.AddSingleton<IUserIdProvider, NotificationUserIdProvider>();
    builder.Services.AddCustomValidation();
    builder.Services.AddOpenApi();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // HttpClient for Razorpay Route API
    builder.Services.AddHttpClient("RazorpayRoute", client =>
    {
        client.BaseAddress = new Uri("https://api.razorpay.com/v1/");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });

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

// Hangfire Configuration
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connection)));
builder.Services.AddHangfireServer();

#region Dependency Injection for Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();

// Course module repositories
builder.Services.AddScoped<ICourseCategoryRepository, CourseCategoryRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseSectionRepository, CourseSectionRepository>();
builder.Services.AddScoped<ILessonRepository, LessonRepository>();
builder.Services.AddScoped<ILessonResourceRepository, LessonResourceRepository>();
builder.Services.AddScoped<IStudentProgressRepository, StudentProgressRepository>();

// Hybrid Learning repositories
builder.Services.AddScoped<ICourseBatchRepository, CourseBatchRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

// Platform Charges & Payout repositories
builder.Services.AddScoped<IPlatformFeeConfigRepository, PlatformFeeConfigRepository>();
builder.Services.AddScoped<IInstructorPayoutAccountRepository, InstructorPayoutAccountRepository>();
builder.Services.AddScoped<IInstructorPayoutRepository, InstructorPayoutRepository>();
builder.Services.AddScoped<IInstructorLinkedAccountRepository, InstructorLinkedAccountRepository>();
builder.Services.AddScoped<IInstructorStakeholderRepository, InstructorStakeholderRepository>();
builder.Services.AddScoped<IInstructorPayoutProductRepository, InstructorPayoutProductRepository>();

// Quiz module repositories
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
builder.Services.AddScoped<IQuizQuestionRepository, QuizQuestionRepository>();
builder.Services.AddScoped<IQuizOptionRepository, QuizOptionRepository>();
builder.Services.AddScoped<IQuizAnswerRepository, QuizAnswerRepository>();
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
builder.Services.AddScoped<IAssignmentSubmissionRepository, AssignmentSubmissionRepository>();
builder.Services.AddScoped<IWishListRepository, WishListRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
builder.Services.AddScoped<ICertificateRepository, CertificateRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IUserNotificationsRepository, UserNotificationsRepository>();
builder.Services.AddScoped<IActivityLogsRepository, ActivityLogsRepository>();
builder.Services.AddScoped<IAuditLogsRepository, AuditLogsRepository>();
builder.Services.AddScoped<IWebhookEventLogRepository, WebhookEventLogRepository>();

// Discussion
builder.Services.AddScoped<IDiscussionRepository,DiscussionRepository>();
builder.Services.AddScoped<IDiscussionReplyRepository,DiscussionReplyRepository>();
builder.Services.AddScoped<IDiscussionLikeRepository,DiscussionLikeRepository>();

#endregion

#region Dependency Injection for Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IAdminUserService, LMSApi.BALLibrary.Services.Users.AdminUserService>();
builder.Services.AddScoped<IUploadService, UploadService>();
builder.Services.AddScoped<ISecureMediaService, SecureMediaService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationHandler, EmailNotificationHandler>();
builder.Services.AddScoped<INotificationHandler, SmsNotificationHandler>();
builder.Services.AddScoped<IEmailJob, EmailJob>();
builder.Services.AddScoped<ICertificateEmailJob, CertificateEmailJob>();
builder.Services.AddScoped<IRegenerateCertificatesJob, RegenerateCertificatesJob>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<ITokenRevocationService, TokenRevocationService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();

// Course module services
builder.Services.AddScoped<ICourseCategoryService, CourseCategoryService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICourseSectionService, CourseSectionService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<ILessonResourceService, LessonResourceService>();
builder.Services.AddScoped<IStudentProgressService, StudentProgressService>();
builder.Services.AddScoped<IAssignmentSubmissionService, AssignmentSubmissionService>();

// Hybrid Learning services
builder.Services.AddScoped<IBatchService, BatchService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Payment Services
builder.Services.AddScoped<IPaymentProvider, RazorpayPaymentProvider>();
builder.Services.AddScoped<IPaymentProvider, StripePaymentProvider>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

// Platform Charges & Payout services
builder.Services.AddScoped<IPlatformFeeService, PlatformFeeService>();
builder.Services.AddScoped<IInstructorPayoutService, InstructorPayoutService>();
builder.Services.AddScoped<IInstructorOnboardingService, InstructorOnboardingService>();
builder.Services.AddScoped<IRevenueService, RevenueService>();

// Quiz module services
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IQuizAttemptService, QuizAttemptService>();
builder.Services.AddScoped<IQuizQuestionService, QuizQuestionService>();
builder.Services.AddScoped<IQuizExpirationService, QuizExpirationService>();

// Ownership Service
builder.Services.AddScoped<IOwnershipService, OwnershipService>();

// Wishlist and Reviews
builder.Services.AddScoped<IWishListService, WishListService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();
builder.Services.AddScoped<IDeadlineNotificationJob, DeadlineNotificationJob>();
builder.Services.AddScoped<IUserNotificationsService, UserNotificationsService>();
builder.Services.AddScoped<INotificationRealtimeService, NotificationRealtimeService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<IAdminLogService, AdminLogService>();
builder.Services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
builder.Services.AddScoped<IWebhookEventService, WebhookEventService>();
builder.Services.AddScoped<ICacheService, CacheService>();

// Discussion Service
builder.Services.AddScoped<IDiscussionService,DiscussionService>();

#endregion

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = builder.Configuration["Redis:InstanceName"];
});

    // Register raw IConnectionMultiplexer for direct Redis access
    builder.Services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
        StackExchange.Redis.ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]!)
    );

    builder.Services.AddAutoMapper(typeof(ApplicationAssemblyReference).Assembly);

    builder.Services.AddScoped<ProfileImageUploadHandler>();
    builder.Services.AddScoped<CourseUploadHandler>();
    builder.Services.AddScoped<AssignmentUploadHandler>();
    builder.Services.AddScoped<LessonUploadHandler>();

    builder.Services.AddJwtAuthentication(builder.Configuration);

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

    // Rate Limiting Configuration
    builder.Services.AddCustomRateLimiting(builder.Configuration);

    // Health Checks for Kubernetes Probes
    builder.Services.AddHealthChecks();

    builder.Services.AddRoleAuthorization();

    // CORS configuration — supports multiple comma-separated origins from config
    var frontendUrl = builder.Configuration["ApplicationUrls:Frontend"] ?? "http://localhost:4200";
    var allowedOrigins = frontendUrl
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });

    builder.Services.AddRequestTimeouts(options =>
    {
        options.AddPolicy("Quick", TimeSpan.FromSeconds(10));
        options.AddPolicy("Normal", TimeSpan.FromSeconds(15));
        options.AddPolicy("Heavy", TimeSpan.FromSeconds(20));
    });

    var app = builder.Build();

    // app.Use(async (context, next) =>
    // {
    //     await Task.Delay(3000);

    //     await next();
    // });

    // ── Exception handling must be FIRST so it wraps every middleware below ──
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<ForgotPasswordEmailExtractorMiddleware>();

    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Note: HTTPS redirection is disabled for HTTP LAN dev access.
    // Re-enable when deploying with TLS/HTTPS.
    // app.UseHttpsRedirection();

    app.UseCors("AllowFrontend");

    app.UseRequestTimeouts();

    app.UseWebSockets(); // MUST be called before UseAuthentication and MapHub

    app.UseAuthentication();
    app.UseMiddleware<TokenRevocationMiddleware>();
    app.UseAuthorization();
    app.UseRateLimiter();

    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() }
    });

app.MapHub<NotificationHub>("/hubs/notification").RequireRateLimiting("SignalRHubConnectNotification");
app.MapHub<VideoProgressHub>("/hubs/video-progress").RequireRateLimiting("SignalRHubConnectVideoProgress");
app.MapHub<QuizProgressHub>("/hubs/quiz-progress");
app.MapControllers();
app.MapHealthChecks("/health");

var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
recurringJobManager.AddOrUpdate<IDeadlineNotificationJob>(
    "DailyDeadlineNotifications",
    job => job.ExecuteAsync(),
    Cron.Daily
);
recurringJobManager.AddOrUpdate<IQuizExpirationService>(
    "QuizExpirationBackgroundJob",
    job => job.ProcessExpiredQuizzesAsync(),
    "*/30 * * * *"
);

    Log.Information("Checking database and Redis connections...");
    using (var scope = app.Services.CreateScope())
    {
        // 1. Check Database connection
        var dbContext = scope.ServiceProvider.GetRequiredService<LMSDbContext>();
        var dbConnected = await dbContext.Database.CanConnectAsync();
        if (!dbConnected)
        {
            Log.Fatal("Could not connect to the database. Stopping server startup.");
            throw new Exception("Database connection failed.");
        }
        Log.Information("Database connection successful.");

        // Automatically apply any pending Entity Framework Core migrations to the database.
        Log.Information("Applying Entity Framework Core migrations...");
        await dbContext.Database.MigrateAsync();
        Log.Information("Database migrations applied successfully.");

        // Automatically run SQL routines
        Log.Information("Applying custom SQL routines...");
        var assembly = typeof(LMSApi.DALLibrary.Contexts.LMSDbContext).Assembly;
        var resourceName = "LMSApi.DALLibrary.routines.sql";
        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream != null)
            {
                using (var reader = new System.IO.StreamReader(stream))
                {
                    var sql = await reader.ReadToEndAsync();
                    await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRawAsync(dbContext.Database, sql);
                    Log.Information("Custom SQL routines applied successfully.");
                }
            }
            else
            {
                Log.Warning("Custom SQL routines file (routines.sql) was not found in the embedded resources.");
            }
        }

        // 2. Check Redis connection
        var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            try
            {
                // Set short timeout for quick fail during startup
                var redisConfig = StackExchange.Redis.ConfigurationOptions.Parse(redisConnectionString);
                redisConfig.ConnectTimeout = 3000;
                using var redis = await StackExchange.Redis.ConnectionMultiplexer.ConnectAsync(redisConfig);
                Log.Information("Redis connection successful.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Could not connect to Redis. Stopping server startup.");
                throw new Exception("Redis connection failed.", ex);
            }
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
