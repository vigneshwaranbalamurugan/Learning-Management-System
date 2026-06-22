using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;
using LMSApi.BALLibrary.Utils;

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
        private readonly INotificationService _notificationService;
        private readonly IWishListRepository _wishListRepository;

        public CourseService(
            ICourseRepository courseRepository,
            ICourseCategoryRepository categoryRepository,
            IUserRepository userRepository,
            IEnrollmentRepository enrollmentRepository,
            IUploadService uploadService,
            IMapper mapper,
            ILogger<CourseService> logger,
            INotificationService notificationService,
            IWishListRepository wishListRepository)
        {
            _courseRepository = courseRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _enrollmentRepository = enrollmentRepository;
            _uploadService = uploadService;
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
            _wishListRepository = wishListRepository;
        }

        public async Task<IEnumerable<CourseResponse>> GetAllCoursesAsync()
        {
            var courses = await _courseRepository.GetPublishedCoursesAsync();
            var responses = _mapper.Map<IEnumerable<CourseResponse>>(courses).ToList();
            await PopulateRatingStatsListAsync(responses);
            return responses;
        }

        public async Task<PagedCourseResponse> GetPublishedCoursesPagedAsync(
            CourseSearchQuery query, int? currentUserId = null)
        {
            if (currentUserId.HasValue)
            {
                var enrollments = await _enrollmentRepository.GetEnrollmentsByUserAsync(currentUserId.Value);
                if (enrollments.Any())
                {
                    var enrolledCourseIds = enrollments.Select(e => e.CourseId).ToList();
                    var existingExcludes = string.IsNullOrWhiteSpace(query.ExcludeCourseIds)
                        ? new List<int>()
                        : query.ExcludeCourseIds.Split(',').Select(int.Parse).ToList();

                    var combinedExcludes = existingExcludes.Union(enrolledCourseIds).Distinct().ToList();
                    query.ExcludeCourseIds = string.Join(",", combinedExcludes);
                }
            }

            var (courses, totalCount) = await _courseRepository.GetPublishedCoursesPagedAsync(query);

            var courseList = _mapper.Map<IEnumerable<CourseResponse>>(courses).ToList();
            
            // Map EnrolledCount and populate rating statistics
            foreach (var response in courseList)
            {
                var original = courses.FirstOrDefault(c => c.Id == response.Id);
                if (original != null)
                {
                    response.EnrolledCount = original.Enrollments?.Count ?? 0;
                }
            }
            
            await PopulateRatingStatsListAsync(courseList);

            var totalPages = (int)System.Math.Ceiling((double)totalCount / query.PageSize);

            return new PagedCourseResponse
            {
                Courses = courseList,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = totalPages
            };
        }

        public async Task<CourseDetailsResponse> GetCourseByIdAsync(int id, int? currentUserId = null, bool isAdmin = false)
        {
            var course = await _courseRepository.GetCourseWithDetailsAsync(id)
                ?? throw new KeyNotFoundException($"Course with id '{id}' not found.");

            if (currentUserId == null || (course.InstructorId != currentUserId && !isAdmin))
            {
                course.Sections = course.Sections
                    .Where(s => s.Status == PublishStatus.Published)
                    .Select(s =>
                    {
                        s.Lessons = s.Lessons
                            .Where(l => l.Status == PublishStatus.Published)
                            .Select(l =>
                            {
                                l.Resources = l.Resources.Where(r => r.Status == PublishStatus.Published).ToList();
                                return l;
                            }).ToList();
                        s.Quizzes = s.Quizzes.Where(q => q.Status == PublishStatus.Published).ToList();
                        s.Assignments = s.Assignments.Where(a => a.Status == PublishStatus.Published).ToList();
                        return s;
                    }).ToList();
            }

            var response = _mapper.Map<CourseDetailsResponse>(course);

            var stats = await _courseRepository.GetCourseRatingStatsAsync(id);
            response.AverageRating = stats.AverageRating;
            response.TotalReviews = stats.TotalReviews;

            if (currentUserId.HasValue)
            {
                response.IsWishlisted = await _wishListRepository.CheckExistsAsync(currentUserId.Value, id);
            }

            return response;
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

            // Verify that the instructor doesn't already have a course with the same title
            var existingCourses = await _courseRepository.GetCoursesByInstructorAsync(instructorId);
            if (existingCourses.Any(c => string.Equals(c.Title.Trim(), request.Title.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("A course with this title already exists for this instructor.");
            }

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

                // Check for duplicate course title for the same instructor (excluding current course id)
                var existingCourses = await _courseRepository.GetCoursesByInstructorAsync(course.InstructorId);
                if (existingCourses.Any(c => c.Id != id && string.Equals(c.Title.Trim(), request.Title.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException("A course with this title already exists for this instructor.");
                }

                course.Title = request.Title;
                course.slug = GenerateSlug(request.Title);
            }

            if (request.CategoryId.HasValue && request.CategoryId > 0 && request.CategoryId != course.CategoryId)
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
            if (request.Level.HasValue) course.Level = request.Level.Value;
            if (request.LanguageId.HasValue) course.LanguageId = request.LanguageId.Value;
            course.EstimatedDuration=request.EstimatedDuration;

            // ─── Hybrid Learning ─────────────────────────────────────────────────
            if (request.CourseAccessType.HasValue) course.CourseAccessType = request.CourseAccessType.Value;
            if (request.DefaultDeadlineDays.HasValue)
                course.DefaultDeadlineDays = request.DefaultDeadlineDays.Value;

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

        public async Task<CourseResponse> PublishCourseAsync(int id, PublishCourseRequest request)
        {
            if (request.Publish)
            {
                var course = await _courseRepository.GetCourseWithDetailsAsync(id)
                    ?? throw new KeyNotFoundException($"Course with id '{id}' not found.");

                if (course.Status == CourseStatus.Published)
                    throw new InvalidOperationException($"Course with id '{id}' is already published.");

                if (course.Status == CourseStatus.PendingApproval)
                    throw new InvalidOperationException($"Course with id '{id}' is already pending approval.");

                // Validation: CohortBased courses must have at least one batch defined before publishing
                if (course.CourseAccessType == CourseAccessType.CohortBased && !course.Batches.Any())
                    throw new InvalidOperationException(
                        $"CohortBased course '{id}' cannot be published without at least one batch. Create a batch first.");

                course.Status = CourseStatus.PendingApproval;

                await _courseRepository.UpdateAsync(course);

                _logger.LogInformation("Course submitted for approval: Id={Id}", id);
                return _mapper.Map<CourseResponse>(course);
            }
            else
            {
                var course = await _courseRepository.GetByIdAsync(id);

                if (course.Status != CourseStatus.Published && course.Status != CourseStatus.PendingApproval)
                    throw new InvalidOperationException($"Course with id '{id}' is not published or pending approval.");

                course.Status = CourseStatus.Draft;
                await _courseRepository.UpdateAsync(course);

                _logger.LogInformation("Course unpublished/cancelled: Id={Id}", id);
                
                var instructor = await _userRepository.GetByIdAsync(course.InstructorId);
                var html = EmailTemplate.GetCourseStatusUpdatedTemplate(
                    instructor.UserProfile?.FirstName ?? instructor.Email,
                    course.Title, "Unpublished", null);
                Message msg = new EmailMessage(instructor.Email, $"Your course '{course.Title}' has been unpublished", html) { IsHtml = true };
                await _notificationService.Send(msg);

                return _mapper.Map<CourseResponse>(course);
            }
        }

        public async Task<CourseResponse> ReviewCourseAsync(int id, ReviewCourseRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var course = await _courseRepository.GetCourseWithDetailsAsync(id)
                ?? throw new KeyNotFoundException($"Course with id '{id}' not found.");

            if (course.Status != CourseStatus.PendingApproval)
                throw new InvalidOperationException($"Course with id '{id}' is not pending approval.");

            if (string.Equals(request.Action, "Approve", StringComparison.OrdinalIgnoreCase))
            {
                course.Status = CourseStatus.Published;
                course.PublishedAt = DateTime.UtcNow;

                if (course.CourseAccessType == CourseAccessType.SelfPaced)
                {
                    foreach (var section in course.Sections)
                    {
                        section.Status = PublishStatus.Published;
                        foreach (var lesson in section.Lessons)
                        {
                            lesson.Status = PublishStatus.Published;
                            foreach (var resource in lesson.Resources)
                            {
                                resource.Status = PublishStatus.Published;
                            }
                        }
                        foreach (var quiz in section.Quizzes) quiz.Status = PublishStatus.Published;
                        foreach (var assignment in section.Assignments) assignment.Status = PublishStatus.Published;
                    }
                }

                await _courseRepository.UpdateAsync(course);

                _logger.LogInformation("Course approved and published: Id={Id}", id);

                var instructor = await _userRepository.GetByIdAsync(course.InstructorId);
                var html = EmailTemplate.GetCourseStatusUpdatedTemplate(
                    instructor.UserProfile?.FirstName ?? instructor.Email,
                    course.Title, "Published", null);
                Message msg = new EmailMessage(instructor.Email, $"Your course '{course.Title}' has been published!", html) { IsHtml = true };
                await _notificationService.Send(msg);

                return _mapper.Map<CourseResponse>(course);
            }
            else if (string.Equals(request.Action, "Reject", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    throw new ArgumentException("Rejection reason is required when rejecting a course.");
                }

                course.Status = CourseStatus.Rejected;
                await _courseRepository.UpdateAsync(course);

                _logger.LogInformation("Course rejected: Id={Id}, Reason={Reason}", id, request.Reason);

                var instructor = await _userRepository.GetByIdAsync(course.InstructorId);
                var html = EmailTemplate.GetCourseStatusUpdatedTemplate(
                    instructor.UserProfile?.FirstName ?? instructor.Email,
                    course.Title, "Rejected", request.Reason);
                Message msg = new EmailMessage(instructor.Email, $"Your course '{course.Title}' was not approved", html) { IsHtml = true };
                await _notificationService.Send(msg);

                return _mapper.Map<CourseResponse>(course);
            }
            else
            {
                throw new ArgumentException($"Invalid action '{request.Action}'. Action must be 'Approve' or 'Reject'.");
            }
        }

        public async Task<IEnumerable<CourseResponse>> GetPendingCoursesAsync()
        {
            var courses = await _courseRepository.GetPendingCoursesAsync();
            var responses = _mapper.Map<IEnumerable<CourseResponse>>(courses).ToList();
            await PopulateRatingStatsListAsync(responses);
            return responses;
        }



        public async Task<IEnumerable<CourseResponse>> GetCoursesByInstructorAsync(int instructorId)
        {
            var courses = await _courseRepository.GetCoursesByInstructorAsync(instructorId);
            var responses = courses == null ? new List<CourseResponse>() : _mapper.Map<IEnumerable<CourseResponse>>(courses).ToList();
            
            if (courses != null)
            {
                foreach (var response in responses)
                {
                    var original = courses.FirstOrDefault(c => c.Id == response.Id);
                    if (original != null)
                    {
                        response.EnrolledCount = original.Enrollments?.Count ?? 0;
                    }
                }
            }

            await PopulateRatingStatsListAsync(responses);
            return responses;
        }

        public async Task<IEnumerable<CourseResponse>> GetCoursesByCategoryAsync(int categoryId)
        {
            var courses = await _courseRepository.GetCoursesByCategoryAsync(categoryId);
            var responses = courses == null ? new List<CourseResponse>() : _mapper.Map<IEnumerable<CourseResponse>>(courses).ToList();
            await PopulateRatingStatsListAsync(responses);
            return responses;
        }

        public async Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryResponse>>(categories);
        }

        public async Task<FiltersMetadataResponse> GetFiltersMetadataAsync()
        {
            var categories = await GetAllCategoriesAsync();
            var languages = await _courseRepository.GetActiveLanguagesAsync();
            var instructors = await _courseRepository.GetActiveInstructorsAsync();

            return new FiltersMetadataResponse
            {
                Categories = categories,
                Languages = languages,
                Instructors = instructors
            };
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private async Task PopulateRatingStatsAsync(CourseResponse response)
        {
            var stats = await _courseRepository.GetCourseRatingStatsAsync(response.Id);
            response.AverageRating = stats.AverageRating;
            response.TotalReviews = stats.TotalReviews;
        }

        private async Task PopulateRatingStatsListAsync(IEnumerable<CourseResponse> responses)
        {
            foreach (var response in responses)
            {
                await PopulateRatingStatsAsync(response);
            }
        }

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
