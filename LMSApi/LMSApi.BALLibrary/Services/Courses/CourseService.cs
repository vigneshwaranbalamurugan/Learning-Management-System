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
        private readonly IMapper _mapper;
        private readonly ILogger<CourseService> _logger;

        public CourseService(
            ICourseRepository courseRepository,
            ICourseCategoryRepository categoryRepository,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<CourseService> logger)
        {
            _courseRepository = courseRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
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

        public async Task<CourseResponse> CreateCourseAsync(CreateCourseRequest request)
        {
            // Validate instructor exists
            await _userRepository.GetByIdAsync(request.InstructorId);

            // Validate category exists
            await _categoryRepository.GetByIdAsync(request.CategoryId);

            var course = _mapper.Map<CourseEntity>(request);
            course.slug = GenerateSlug(request.Title);
            course.Status = CourseStatus.Draft;

            await _courseRepository.AddAsync(course);

            _logger.LogInformation("Course Created: '{Title}' by InstructorId={InstructorId}", request.Title, request.InstructorId);

            return _mapper.Map<CourseResponse>(course);
        }

        public async Task<CourseResponse> UpdateCourseAsync(int id, UpdateCourseRequest request)
        {
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
            if (request.ThumbnailUrl != null) course.ThumbnailUrl = request.ThumbnailUrl;
            if (request.IntroVideoUrl != null) course.IntroVideoUrl = request.IntroVideoUrl;
            if (request.Requirements != null) course.Requirements = request.Requirements;
            if (request.LearningOutcomes != null) course.LearningOutcomes = request.LearningOutcomes;
            if (request.EstimatedDuration.HasValue) course.EstimatedDuration = request.EstimatedDuration.Value;
            if (request.Level.HasValue) course.Level = request.Level.Value;
            if (request.Language.HasValue) course.Language = request.Language.Value;

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
            return _mapper.Map<IEnumerable<CourseResponse>>(courses);
        }

        public async Task<IEnumerable<CourseResponse>> GetCoursesByCategoryAsync(int categoryId)
        {
            var courses = await _courseRepository.GetCoursesByCategoryAsync(categoryId);
            return _mapper.Map<IEnumerable<CourseResponse>>(courses);
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
