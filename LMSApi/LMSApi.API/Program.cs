using LMSApi.DALLibrary.Contexts;
using LMSApi.API.Extensions;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.DALLibrary.Repositories;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Services;
using LMSApi.BALLibrary.Services.Authentication;
using LMSApi.BALLibrary.Services.Profile;
using LMSApi.BALLibrary.Services.Upload;
using LMSApi.BALLibrary.Mappers;
using LMSApi.API.Middlewares;
using LMSApi.API.Handlers;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;

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

// Quiz module repositories
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
builder.Services.AddScoped<IQuizQuestionRepository, QuizQuestionRepository>();
builder.Services.AddScoped<IQuizOptionRepository, QuizOptionRepository>();
builder.Services.AddScoped<IQuizAnswerRepository, QuizAnswerRepository>();
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
builder.Services.AddScoped<IAssignmentSubmissionRepository, AssignmentSubmissionRepository>();
#endregion

#region Dependency Injection for Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IUploadService, UploadService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationHandler, EmailNotificationHandler>();
builder.Services.AddScoped<INotificationHandler, SmsNotificationHandler>();
builder.Services.AddSingleton<ITokenService, TokenService>();

// Course module services
builder.Services.AddScoped<ICourseCategoryService, CourseCategoryService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICourseSectionService, CourseSectionService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<ILessonResourceService, LessonResourceService>();
builder.Services.AddScoped<IStudentProgressService, StudentProgressService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();

// Hybrid Learning services
builder.Services.AddScoped<IBatchService, BatchService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Quiz module services
builder.Services.AddScoped<IQuizService, QuizService>();
#endregion

builder.Services.AddAutoMapper(typeof(ApplicationAssemblyReference).Assembly);

builder.Services.AddScoped<ProfileImageUploadHandler>();
builder.Services.AddScoped<CourseUploadHandler>();
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

builder.Services.AddRoleAuthorization();

var app = builder.Build();

// ── Exception handling must be FIRST so it wraps every middleware below ──
app.UseMiddleware<ExceptionHandlingMiddleware>();

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

app.MapControllers();

app.Run();
