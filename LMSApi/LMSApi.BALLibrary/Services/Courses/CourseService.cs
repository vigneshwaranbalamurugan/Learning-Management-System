using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseCategoryRepository _categoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IUploadService _uploadService;
        private readonly IMapper _mapper;
        private readonly ILogger<CourseService> _logger;

        public CourseService(
            ICourseRepository courseRepository,
            ICourseCategoryRepository categoryRepository,
            IUserRepository userRepository,
            IEnrollmentRepository enrollmentRepository,
            IUploadService uploadService,
            IMapper mapper,
            ILogger<CourseService> logger)
        {
            _courseRepository = courseRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _enrollmentRepository = enrollmentRepository;
            _uploadService = uploadService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<CourseResponse>> GetAllCoursesAsync()
        {
            var courses = await _courseRepository.GetPublishedCoursesAsync();
            return _mapper.Map<IEnumerable<CourseResponse>>(courses);
        }

        public async Task<CourseDetailsResponse> GetCourseByIdAsync(int id)
        {
            var course = await _courseRepository.GetCourseWithDetailsAsync(id)
                ?? throw new KeyNotFoundException($"Course with id '{id}' not found.");
            return _mapper.Map<CourseDetailsResponse>(course);
        }

        /// <summary>
        /// Creates a new course. instructorId is always sourced from the caller's JWT token.
        /// </summary>
        public async Task<CourseResponse> CreateCourseAsync(
            int instructorId,
            CreateCourseRequest request,
            Stream? thumbnailStream = null, string? thumbnailFileName = null,
            Stream? videoStream = null, string? videoFileName = null)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Title)) throw new ArgumentException("Course title cannot be null or empty.", nameof(request.Title));
            if (request.CategoryId <= 0) throw new ArgumentException("Valid CategoryId must be provided.", nameof(request.CategoryId));
            if (request.IsPremium && (!request.Price.HasValue || request.Price <= 0))
                throw new ArgumentException("Price must be provided and greater than zero when IsPremium is true.", nameof(request.Price));
            // Verify the calling user actually exists
            await _userRepository.GetByIdAsync(instructorId);

            // Verify category exists
            await _categoryRepository.GetByIdAsync(request.CategoryId);

            var course = _mapper.Map<Courses>(request);
            course.InstructorId = instructorId;    // always from token, never from client
            course.slug = GenerateSlug(request.Title);
            course.Status = CourseStatus.Draft;

            if (thumbnailStream != null && thumbnailFileName != null)
            {
                course.ThumbnailUrl = await _uploadService.UploadCourseThumbnailAsync(
                    thumbnailStream, thumbnailFileName, $"courses/{course.Id}/thumbnail");
                _logger.LogInformation("Thumbnail uploaded on create: CourseId={CourseId}", course.Id);
            }

            if (videoStream != null && videoFileName != null)
            {
                course.IntroVideoUrl = await _uploadService.UploadCourseIntroVideoAsync(
                    videoStream, videoFileName, $"courses/{course.Id}/intro-video");
                _logger.LogInformation("Intro video uploaded on create: CourseId={CourseId}", course.Id);
            }
            course.ThumbnailUrl ??= string.Empty;
            course.IntroVideoUrl ??= string.Empty;
            course.Requirements ??= string.Empty;
            course.LearningOutcomes ??= string.Empty;
            course.Description ??= string.Empty;
            await _courseRepository.AddAsync(course);
            _logger.LogInformation("Course Created: '{Title}' by InstructorId={InstructorId}", request.Title, instructorId);

            return _mapper.Map<CourseResponse>(course);
        }

        /// <summary>
        /// Updates a course. callerUserId (from JWT) must match the course's InstructorId, unless the caller is Admin.
        /// </summary>
        public async Task<CourseResponse> UpdateCourseAsync(
            int id, UpdateCourseRequest request,
            Stream? thumbnailStream = null, string? thumbnailFileName = null,
            Stream? videoStream = null, string? videoFileName = null)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var course = await _courseRepository.GetByIdAsync(id);

            if (request.Title != null)
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                    throw new ArgumentException("Title cannot be empty.", nameof(request.Title));
                course.Title = request.Title;
                course.slug = GenerateSlug(request.Title);
            }

            if (request.CategoryId.HasValue)
            {
                await _categoryRepository.GetByIdAsync(request.CategoryId.Value);
                course.CategoryId = request.CategoryId.Value;
            }

            if (request.Description != null) course.Description = request.Description;

            bool finalIsPremium = request.IsPremium ?? course.IsPremium;
            decimal finalPrice = request.Price ?? course.Price ?? 0m;

            if (finalIsPremium && finalPrice <= 0)
                throw new ArgumentException("Price must be provided and greater than zero when IsPremium is true.");

            if (request.Price.HasValue) course.Price = request.Price.Value;
            if (request.IsPremium.HasValue) course.IsPremium = request.IsPremium.Value;

            if (request.Requirements != null) course.Requirements = request.Requirements;
            if (request.LearningOutcomes != null) course.LearningOutcomes = request.LearningOutcomes;
            if (request.EstimatedDuration.HasValue) course.EstimatedDuration = request.EstimatedDuration.Value;
            if (request.Level.HasValue) course.Level = request.Level.Value;
            if (request.Language.HasValue) course.Language = request.Language.Value;

            // ─── Hybrid Learning ─────────────────────────────────────────────────
            if (request.CourseAccessType.HasValue) course.CourseAccessType = request.CourseAccessType.Value;
            if (request.DefaultAssignmentDeadlineDays.HasValue)
                course.DefaultAssignmentDeadlineDays = request.DefaultAssignmentDeadlineDays.Value;

            if (thumbnailStream != null && thumbnailFileName != null)
            {
                course.ThumbnailUrl = await _uploadService.UploadCourseThumbnailAsync(
                    thumbnailStream, thumbnailFileName, $"courses/{course.Id}/thumbnail");
                _logger.LogInformation("Thumbnail updated: CourseId={CourseId}", course.Id);
            }

            if (videoStream != null && videoFileName != null)
            {
                course.IntroVideoUrl = await _uploadService.UploadCourseIntroVideoAsync(
                    videoStream, videoFileName, $"courses/{course.Id}/intro-video");
                _logger.LogInformation("Intro video updated: CourseId={CourseId}", course.Id);
            }

            await _courseRepository.UpdateAsync(course);
            _logger.LogInformation("Course Updated: Id={Id}", id);

            return _mapper.Map<CourseResponse>(course);
        }

        public async Task DeleteCourseAsync(int id)
        {
            await _courseRepository.GetByIdAsync(id); // throws if not found

            var hasEnrollments = await _enrollmentRepository.HasEnrollmentsByCourseAsync(id);
            if (hasEnrollments)
            {
                throw new InvalidOperationException($"Course '{id}' cannot be deleted because it has active enrollments.");
            }

            await _courseRepository.DeleteAsync(id);
            _logger.LogInformation("Course Deleted: Id={Id}", id);
        }

        public async Task<CourseResponse> PublishCourseAsync(int id)
        {
            var course = await _courseRepository.GetCourseWithDetailsAsync(id)
                ?? throw new KeyNotFoundException($"Course with id '{id}' not found.");

            if (course.Status == CourseStatus.Published)
                throw new InvalidOperationException($"Course with id '{id}' is already published.");

            // Validation: CohortBased courses must have at least one batch defined before publishing
            if (course.CourseAccessType == CourseAccessType.CohortBased && !course.Batches.Any())
                throw new InvalidOperationException(
                    $"CohortBased course '{id}' cannot be published without at least one batch. Create a batch first.");

            course.Status = CourseStatus.Published;
            course.PublishedAt = DateTime.UtcNow;

            if (course.CourseAccessType == CourseAccessType.SelfPaced)
            {
                foreach (var section in course.Sections)
                {
                    section.IsPublished = true;
                    foreach (var lesson in section.Lessons) lesson.IsPublished = true;
                    foreach (var quiz in section.Quizzes) quiz.IsPublished = true;
                    foreach (var assignment in section.Assignments) assignment.IsPublished = true;
                }
            }

            await _courseRepository.UpdateAsync(course);

            _logger.LogInformation("Course Published: Id={Id}", id);
            return _mapper.Map<CourseResponse>(course);
        }


        public async Task<CourseResponse> UnpublishCourseAsync(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);

            if (course.Status != CourseStatus.Published)
                throw new InvalidOperationException($"Course with id '{id}' is not currently published.");

            course.Status = CourseStatus.Draft;
            await _courseRepository.UpdateAsync(course);

            _logger.LogInformation("Course Unpublished: Id={Id}", id);
            return _mapper.Map<CourseResponse>(course);
        }

        public async Task<IEnumerable<CourseResponse>> GetCoursesByInstructorAsync(int instructorId)
        {
            var courses = await _courseRepository.GetCoursesByInstructorAsync(instructorId);
            return courses == null ? Enumerable.Empty<CourseResponse>() : _mapper.Map<IEnumerable<CourseResponse>>(courses);
        }

        public async Task<IEnumerable<CourseResponse>> GetCoursesByCategoryAsync(int categoryId)
        {
            var courses = await _courseRepository.GetCoursesByCategoryAsync(categoryId);
            return courses == null ? Enumerable.Empty<CourseResponse>() : _mapper.Map<IEnumerable<CourseResponse>>(courses);
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private static string GenerateSlug(string title)
        {
            return title.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace("&", "and")
                + "-" + Guid.NewGuid().ToString("N")[..6];
        }
    }
}
