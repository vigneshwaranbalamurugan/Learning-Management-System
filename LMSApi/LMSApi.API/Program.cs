using LMSApi.DALLibrary.Contexts;
using LMSApi.API.Extensions;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.DALLibrary.Repositories;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Services;
using LMSApi.BALLibrary.Services.Upload;
using LMSApi.BALLibrary.Services.Notification;
using LMSApi.BALLibrary.Mappers;
using LMSApi.API.Middlewares;
using LMSApi.API.Handlers;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Hangfire.PostgreSql;
using LMSApi.API.Filters;
using LMSApi.API.Hubs;
using LMSApi.API.Services;
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
#endregion

#region Dependency Injection for Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IUploadService, UploadService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationHandler, EmailNotificationHandler>();
builder.Services.AddScoped<INotificationHandler, SmsNotificationHandler>();
builder.Services.AddScoped<IEmailJob, EmailJob>();
builder.Services.AddScoped<ICertificateEmailJob, CertificateEmailJob>();
builder.Services.AddSingleton<ITokenService, TokenService>();

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

// Platform Charges & Payout services
builder.Services.AddScoped<IPlatformFeeService, PlatformFeeService>();
builder.Services.AddScoped<IInstructorPayoutService, InstructorPayoutService>();
builder.Services.AddScoped<IInstructorOnboardingService, InstructorOnboardingService>();
builder.Services.AddScoped<IRevenueService, RevenueService>();

// Quiz module services
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IQuizAttemptService, QuizAttemptService>();
builder.Services.AddScoped<IQuizQuestionService, QuizQuestionService>();

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
#endregion

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
builder.Services.AddCustomRateLimiting();

builder.Services.AddRoleAuthorization();

var app = builder.Build();

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

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// app.UseHangfireDashboard("/hangfire", new DashboardOptions
// {
//     Authorization = new[] { new HangfireAuthorizationFilter() }
// });

app.UseHangfireDashboard("/hangfire");

app.MapHub<NotificationHub>("/hubs/notification").RequireRateLimiting("SignalRHubConnect");
app.MapHub<VideoProgressHub>("/hubs/video-progress").RequireRateLimiting("SignalRHubConnect");
app.MapControllers();

var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
recurringJobManager.AddOrUpdate<IDeadlineNotificationJob>(
    "DailyDeadlineNotifications",
    job => job.ExecuteAsync(),
    Cron.Daily
);

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
