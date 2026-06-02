using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;
using CourseEntity = LMSApi.ModelLibrary.Models.Courses;

namespace LMSApi.BALLibrary.Services.Courses
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseCategoryRepository _categoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUploadService _uploadService;
        private readonly IMapper _mapper;
        private readonly ILogger<CourseService> _logger;

        public CourseService(
            ICourseRepository courseRepository,
            ICourseCategoryRepository categoryRepository,
            IUserRepository userRepository,
            IUploadService uploadService,
            IMapper mapper,
            ILogger<CourseService> logger)
        {
            _courseRepository = courseRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
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

            // Verify the calling user actually exists
            await _userRepository.GetByIdAsync(instructorId);

            // Verify category exists
            await _categoryRepository.GetByIdAsync(request.CategoryId);

            var course = _mapper.Map<CourseEntity>(request);
            course.InstructorId = instructorId;    // always from token, never from client
            course.slug = GenerateSlug(request.Title);
            course.Status = CourseStatus.Draft;

            // Persist first so we have a DB id for the Cloudinary public_id
            await _courseRepository.AddAsync(course);

            var needsUpdate = false;

            if (thumbnailStream != null && thumbnailFileName != null)
            {
                course.ThumbnailUrl = await _uploadService.UploadCourseThumbnailAsync(
                    thumbnailStream, thumbnailFileName, $"courses/{course.Id}/thumbnail");
                needsUpdate = true;
                _logger.LogInformation("Thumbnail uploaded on create: CourseId={CourseId}", course.Id);
            }

            if (videoStream != null && videoFileName != null)
            {
                course.IntroVideoUrl = await _uploadService.UploadCourseIntroVideoAsync(
                    videoStream, videoFileName, $"courses/{course.Id}/intro-video");
                needsUpdate = true;
                _logger.LogInformation("Intro video uploaded on create: CourseId={CourseId}", course.Id);
            }

            if (needsUpdate)
                await _courseRepository.UpdateAsync(course);

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
                course.Title = request.Title;
                course.slug = GenerateSlug(request.Title);
            }

            if (request.CategoryId.HasValue)
            {
                await _categoryRepository.GetByIdAsync(request.CategoryId.Value);
                course.CategoryId = request.CategoryId.Value;
            }

            if (request.Description != null) course.Description = request.Description;
            if (request.Price.HasValue) course.Price = request.Price.Value;
            if (request.IsPremium.HasValue) course.IsPremium = request.IsPremium.Value;
            if (request.Requirements != null) course.Requirements = request.Requirements;
            if (request.LearningOutcomes != null) course.LearningOutcomes = request.LearningOutcomes;
            if (request.EstimatedDuration.HasValue) course.EstimatedDuration = request.EstimatedDuration.Value;
            if (request.Level.HasValue) course.Level = request.Level.Value;
            if (request.Language.HasValue) course.Language = request.Language.Value;

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
            await _courseRepository.GetByIdAsync(id);
            await _courseRepository.DeleteAsync(id);
            _logger.LogInformation("Course Deleted: Id={Id}", id);
        }

        public async Task<CourseResponse> PublishCourseAsync(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id);

            if (course.Status == CourseStatus.Published)
                throw new InvalidOperationException($"Course with id '{id}' is already published.");

            course.Status = CourseStatus.Published;
            course.PublishedAt = DateTime.UtcNow;
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
