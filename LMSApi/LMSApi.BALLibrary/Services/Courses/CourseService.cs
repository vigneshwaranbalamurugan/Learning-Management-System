using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using LMSApi.BALLibrary.Utils;

namespace LMSApi.BALLibrary.Services
{
    public class CourseService : ICourseService
    {
        private const string CacheKeyDetailPrefix = "course:detail:";
        private const string CacheKeySlugPrefix = "course:slug:";
        private const string CacheKeyStatsPrefix = "course:stats:";

        private readonly ICourseRepository _courseRepository;
        private readonly ICourseCategoryRepository _categoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IUploadService _uploadService;
        private readonly IMapper _mapper;
        private readonly ILogger<CourseService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IWishListRepository _wishListRepository;
        private readonly IUserNotificationsService _userNotificationsService;
        private readonly IReviewRepository _reviewRepository;
        private readonly ICacheService _cacheService;
        private readonly int _detailTtlMinutes;
        private readonly int _statsTtlMinutes;

        public CourseService(
            ICourseRepository courseRepository,
            ICourseCategoryRepository categoryRepository,
            IUserRepository userRepository,
            IEnrollmentRepository enrollmentRepository,
            IUploadService uploadService,
            IMapper mapper,
            ILogger<CourseService> logger,
            INotificationService notificationService,
            IWishListRepository wishListRepository,
            IUserNotificationsService userNotificationsService,
            IReviewRepository reviewRepository,
            ICacheService cacheService,
            IConfiguration configuration)
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
            _userNotificationsService = userNotificationsService;
            _reviewRepository = reviewRepository;
            _cacheService = cacheService;
            _detailTtlMinutes = configuration.GetValue<int>("Cache:CourseDetailTtlMinutes", 30);
            _statsTtlMinutes = configuration.GetValue<int>("Cache:CourseStatsTtlMinutes", 10);
        }

        public async Task<IEnumerable<CourseResponse>> GetAllCoursesAsync()
        {
            var courses = await _courseRepository.GetPublishedCoursesAsync();
            var responses = _mapper.Map<IEnumerable<CourseResponse>>(courses).ToList();
            await PopulateRatingStatsListAsync(responses);
            return responses;
        }

        public async Task<PagedCourseListResponse> GetAllCoursesPagedAsync(CourseSearchQuery query)
        {
            var (courses, totalCount) = await _courseRepository.GetAllCoursesPagedAsync(query);

            var courseList = _mapper.Map<IEnumerable<CourseListItemResponse>>(courses).ToList();
            
            var statsDict = await _courseRepository.GetRatingStatsBatchAsync(courseList.Select(c => c.Id));
            foreach (var c in courseList)
            {
                if (statsDict.TryGetValue(c.Id, out var stats))
                {
                    c.AverageRating = stats.AverageRating;
                    c.TotalReviews = stats.TotalReviews;
                }
            }

            var totalPages = (int)System.Math.Ceiling((double)totalCount / query.PageSize);

            return new PagedCourseListResponse
            {
                Courses = courseList,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = totalPages
            };
        }

        public async Task<PagedCourseListResponse> GetPublishedCoursesPagedAsync(
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

            var courseList = _mapper.Map<IEnumerable<CourseListItemResponse>>(courses).ToList();
            
            var statsDict = await _courseRepository.GetRatingStatsBatchAsync(courseList.Select(c => c.Id));
            foreach (var c in courseList)
            {
                if (statsDict.TryGetValue(c.Id, out var stats))
                {
                    c.AverageRating = stats.AverageRating;
                    c.TotalReviews = stats.TotalReviews;
                }
            }

            var totalPages = (int)System.Math.Ceiling((double)totalCount / query.PageSize);

            return new PagedCourseListResponse
            {
                Courses = courseList,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = totalPages
            };
        }

        public async Task<CourseResponse> GetCourseByIdAsync(int id, int? currentUserId = null, bool isAdmin = false)
        {
            async Task<CoursePreviewResponse> GetCachedPreviewAsync()
            {
                return await _cacheService.GetOrSetAsync(
                    $"{CacheKeyDetailPrefix}{id}",
                    async () => 
                    {
                        var c = await _courseRepository.GetCourseWithDetailsAsync(id)
                            ?? throw new KeyNotFoundException($"Course with id '{id}' not found.");

                        var reviewsData = await _reviewRepository.GetByCourseAsync(c.Id);
                        var reviewResponses = reviewsData.Select(r => new ReviewResponse
                        {
                            Id = r.Id,
                            CourseId = r.CourseId,
                            UserId = r.UserId,
                            UserName = r.User?.Email ?? "",
                            Rating = r.Rating,
                            ReviewText = r.Review,
                            CreatedAt = r.CreatedAt,
                            UpdatedAt = r.UpdatedAt
                        }).ToList();

                        c.Sections = c.Sections
                            .Where(s => s.Status == PublishStatus.Published)
                            .Select(s =>
                            {
                                s.Lessons = s.Lessons.Where(l => l.Status == PublishStatus.Published).ToList();
                                s.Quizzes = s.Quizzes.Where(q => q.Status == PublishStatus.Published).ToList();
                                s.Assignments = s.Assignments.Where(a => a.Status == PublishStatus.Published).ToList();
                                return s;
                            }).ToList();

                        var preview = _mapper.Map<CoursePreviewResponse>(c);
                        preview.EnrolledCount = c.Enrollments?.Count ?? 0;

                        var pStats = await _courseRepository.GetCourseRatingStatsAsync(c.Id);
                        preview.AverageRating = pStats.AverageRating;
                        preview.TotalReviews = pStats.TotalReviews;
                        preview.IsEnrolled = false;
                        preview.Reviews = reviewResponses;
                        
                        return preview;
                    },
                    TimeSpan.FromMinutes(_detailTtlMinutes));
            }

            if (currentUserId == null)
            {
                return await GetCachedPreviewAsync();
            }

            var course = await _courseRepository.GetCourseWithDetailsAsync(id)
                ?? throw new KeyNotFoundException($"Course with id '{id}' not found.");

            var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(currentUserId.Value, course.Id);
            bool isEnrolled = enrollment != null;

            if (!isEnrolled && course.InstructorId != currentUserId && !isAdmin)
            {
                // Clone the cached preview so we don't mutate the cached instance directly
                var cachedPreview = await GetCachedPreviewAsync();
                var preview = Newtonsoft.Json.JsonConvert.DeserializeObject<CoursePreviewResponse>(Newtonsoft.Json.JsonConvert.SerializeObject(cachedPreview));
                if (preview != null)
                {
                    preview.IsWishlisted = await _wishListRepository.CheckExistsAsync(currentUserId.Value, course.Id);
                    return preview;
                }
                return cachedPreview;
            }

            var response = _mapper.Map<CourseDetailsResponse>(course);
            response.EnrolledCount = course.Enrollments?.Count ?? 0;

            var stats = await _courseRepository.GetCourseRatingStatsAsync(id);
            response.AverageRating = stats.AverageRating;
            response.TotalReviews = stats.TotalReviews;

            response.IsWishlisted = await _wishListRepository.CheckExistsAsync(currentUserId.Value, id);
            
            response.IsEnrolled = isEnrolled;
            if (enrollment != null)
            {
                response.EnrollmentId = enrollment.Id;
                response.EnrollmentProgress = (double)enrollment.ProgressPercentage;
            }

            var rData = await _reviewRepository.GetByCourseAsync(course.Id);
            response.Reviews = rData.Select(r => new ReviewResponse
            {
                Id = r.Id,
                CourseId = r.CourseId,
                UserId = r.UserId,
                UserName = r.User?.Email ?? "",
                Rating = r.Rating,
                ReviewText = r.Review,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToList();
            
            response.HasNonExpiredEnrollments = await _enrollmentRepository.HasNonExpiredEnrollmentsByCourseAsync(id);
            response.HasActiveEnrollments = await _enrollmentRepository.HasActiveOnlyEnrollmentsByCourseAsync(id);

            return response;
        }

        public async Task<CourseResponse> GetCourseBySlugAsync(string slug, int? currentUserId = null, bool isAdmin = false)
        {
            async Task<CoursePreviewResponse> GetCachedPreviewAsync()
            {
                return await _cacheService.GetOrSetAsync(
                    $"{CacheKeySlugPrefix}{slug}",
                    async () => 
                    {
                        var c = await _courseRepository.GetCourseBySlugWithDetailsAsync(slug)
                            ?? throw new KeyNotFoundException($"Course with slug '{slug}' not found.");

                        var reviewsData = await _reviewRepository.GetByCourseAsync(c.Id);
                        var reviewResponses = reviewsData.Select(r => new ReviewResponse
                        {
                            Id = r.Id,
                            CourseId = r.CourseId,
                            UserId = r.UserId,
                            UserName = r.User?.Email ?? "",
                            Rating = r.Rating,
                            ReviewText = r.Review,
                            CreatedAt = r.CreatedAt,
                            UpdatedAt = r.UpdatedAt
                        }).ToList();

                        c.Sections = c.Sections
                            .Where(s => s.Status == PublishStatus.Published)
                            .Select(s =>
                            {
                                s.Lessons = s.Lessons.Where(l => l.Status == PublishStatus.Published).ToList();
                                s.Quizzes = s.Quizzes.Where(q => q.Status == PublishStatus.Published).ToList();
                                s.Assignments = s.Assignments.Where(a => a.Status == PublishStatus.Published).ToList();
                                return s;
                            }).ToList();

                        var preview = _mapper.Map<CoursePreviewResponse>(c);
                        preview.EnrolledCount = c.Enrollments?.Count ?? 0;

                        var pStats = await _courseRepository.GetCourseRatingStatsAsync(c.Id);
                        preview.AverageRating = pStats.AverageRating;
                        preview.TotalReviews = pStats.TotalReviews;
                        preview.IsEnrolled = false;
                        preview.Reviews = reviewResponses;
                        
                        return preview;
                    },
                    TimeSpan.FromMinutes(_detailTtlMinutes));
            }

            if (currentUserId == null)
            {
                return await GetCachedPreviewAsync();
            }

            var course = await _courseRepository.GetCourseBySlugWithDetailsAsync(slug)
                ?? throw new KeyNotFoundException($"Course with slug '{slug}' not found.");

            var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(currentUserId.Value, course.Id);
            bool isEnrolled = enrollment != null;

            if (!isEnrolled && course.InstructorId != currentUserId && !isAdmin)
            {
                // Clone the cached preview
                var cachedPreview = await GetCachedPreviewAsync();
                var preview = Newtonsoft.Json.JsonConvert.DeserializeObject<CoursePreviewResponse>(Newtonsoft.Json.JsonConvert.SerializeObject(cachedPreview));
                if (preview != null)
                {
                    preview.IsWishlisted = await _wishListRepository.CheckExistsAsync(currentUserId.Value, course.Id);
                    return preview;
                }
                return cachedPreview;
            }

            var response = _mapper.Map<CourseDetailsResponse>(course);
            response.EnrolledCount = course.Enrollments?.Count ?? 0;

            var stats = await _courseRepository.GetCourseRatingStatsAsync(course.Id);
            response.AverageRating = stats.AverageRating;
            response.TotalReviews = stats.TotalReviews;

            response.IsWishlisted = await _wishListRepository.CheckExistsAsync(currentUserId.Value, course.Id);

            response.IsEnrolled = isEnrolled;
            if (enrollment != null)
            {
                response.EnrollmentId = enrollment.Id;
                response.EnrollmentProgress = (double)enrollment.ProgressPercentage;
            }

            var rData = await _reviewRepository.GetByCourseAsync(course.Id);
            response.Reviews = rData.Select(r => new ReviewResponse
            {
                Id = r.Id,
                CourseId = r.CourseId,
                UserId = r.UserId,
                UserName = r.User?.Email ?? "",
                Rating = r.Rating,
                ReviewText = r.Review,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToList();

            response.HasNonExpiredEnrollments = await _enrollmentRepository.HasNonExpiredEnrollmentsByCourseAsync(course.Id);
            response.HasActiveEnrollments = await _enrollmentRepository.HasActiveOnlyEnrollmentsByCourseAsync(course.Id);

            return response;
        }

        /// <summary>
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
            course.ThumbnailUrl ??= string.Empty;
            course.IntroVideoUrl ??= string.Empty;
            course.Requirements ??= string.Empty;
            course.LearningOutcomes ??= string.Empty;

            // Save first so course.Id is generated by the DB before we build the Cloudinary publicId
            await _courseRepository.AddAsync(course);

            bool needsUpdate = false;

            if (thumbnailStream != null && thumbnailFileName != null)
            {
                // Bug fix: use course.Id (now set) so each course gets a unique Cloudinary key
                course.ThumbnailUrl = await _uploadService.UploadCourseThumbnailAsync(
                    thumbnailStream, thumbnailFileName, $"courses/{course.Id}/thumbnail");
                _logger.LogInformation("Thumbnail uploaded on create: CourseId={CourseId}", course.Id);
                needsUpdate = true;
            }

            if (videoStream != null && videoFileName != null)
            {
                var uploadResult = await _uploadService.UploadCourseIntroVideoAsync(
                    videoStream, videoFileName, $"courses/{course.Id}/intro-video");
                course.IntroVideoUrl = uploadResult.Url;
                _logger.LogInformation("Intro video uploaded on create: CourseId={CourseId}", course.Id);
                needsUpdate = true;
            }

            // Persist the Cloudinary URLs back to the DB if any were uploaded
            if (needsUpdate)
            {
                await _courseRepository.UpdateAsync(course);
            }

            _logger.LogInformation("Course Created: '{Title}' by InstructorId={InstructorId}", request.Title, instructorId);

            // Bug #5: Invalidate global stats cache when a new course is created
            await _cacheService.InvalidateAsync(CacheKeyStatsPrefix + "global");

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
                var uploadResult = await _uploadService.UploadCourseIntroVideoAsync(
                    videoStream, videoFileName, $"courses/{course.Id}/intro-video");
                course.IntroVideoUrl = uploadResult.Url;
                _logger.LogInformation("Intro video updated: CourseId={CourseId}", course.Id);
            }

            await _courseRepository.UpdateAsync(course);
            _logger.LogInformation("Course Updated: Id={Id}", id);

            // Bug #2 + #4: Invalidate both ID-based and slug-based cache entries
            await InvalidateCourseCache(id, course.slug);

            return _mapper.Map<CourseResponse>(course);
        }

        public async Task DeleteCourseAsync(int id)
        {
            await _courseRepository.GetByIdAsync(id); // throws if not found

            var hasNonExpired = await _enrollmentRepository.HasNonExpiredEnrollmentsByCourseAsync(id);
            if (hasNonExpired)
            {
                throw new InvalidOperationException("Course cannot be permanently deleted because learners are enrolled. Use soft-delete instead.");
            }

            var courseForCache = await _courseRepository.GetByIdAsync(id);
            var slug = courseForCache?.slug;

            await _courseRepository.DeleteAsync(id);
            _logger.LogInformation("Course Deleted: Id={Id}", id);

            // Bug #2 + #4 + #5: Invalidate both cache keys and global stats
            await InvalidateCourseCache(id, slug);
            await _cacheService.InvalidateAsync(CacheKeyStatsPrefix + "global");
        }

        public async Task SoftDeleteCourseAsync(int id)
        {
            var course = await _courseRepository.GetByIdAsync(id); // throws if not found
            await _courseRepository.SoftDeleteCourseAsync(id);
            _logger.LogInformation("Course SoftDeleted: Id={Id}", id);

            // Bug #2 + #4: Invalidate both cache keys
            await InvalidateCourseCache(id, course?.slug);
        }

        public async Task<CourseResponse> ArchiveCourseAsync(int id, ArchiveCourseRequest request)
        {
            var course = await _courseRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Course with id '{id}' not found.");

            if (request.Archive)
            {
                course.Status = CourseStatus.Archived;
                _logger.LogInformation("Course Archived: Id={Id}", id);
            }
            else
            {
                var hasNonExpired = await _enrollmentRepository.HasNonExpiredEnrollmentsByCourseAsync(id);
                if (hasNonExpired)
                    throw new InvalidOperationException($"Course '{course.Title}' cannot be unarchived because it has active enrollments.");

                course.Status = CourseStatus.Draft;
                _logger.LogInformation("Course Unarchived (reverted to Draft): Id={Id}", id);
            }

            await _courseRepository.UpdateAsync(course);

            // Invalidate both cache keys
            await InvalidateCourseCache(id, course.slug);

            if (request.Archive)
            {
                var instructor = await _userRepository.GetByIdAsync(course.InstructorId);
                var reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason;
                var html = EmailTemplate.GetCourseStatusUpdatedTemplate(
                    instructor.UserProfile?.FirstName ?? instructor.Email,
                    course.Title, "Archived", reason);
                Message msg = new EmailMessage(instructor.Email, $"Your course '{course.Title}' has been archived", html) { IsHtml = true };
                await _notificationService.Send(msg);

                try
                {
                    await _userNotificationsService.CreateAndSendNotificationAsync(
                        userId: course.InstructorId,
                        title: "Course Archived",
                        message: reason != null ? $"Your course '{course.Title}' has been archived. Reason: {reason}" : $"Your course '{course.Title}' has been archived.",
                        type: NotificationType.General,
                        redirectUrl: $"/courses/{course.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send course archived realtime notification to Instructor {InstructorId}", course.InstructorId);
                }
            }

            return _mapper.Map<CourseResponse>(course);
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

                // If course has active enrollments, it was previously published and then archived or unarchived. We don't allow republishing.
                var hasNonExpired = await _enrollmentRepository.HasNonExpiredEnrollmentsByCourseAsync(id);
                if (hasNonExpired)
                    throw new InvalidOperationException($"Course '{course.Title}' has active enrollments and cannot be published again.");

                // Validation: CohortBased courses must have at least one batch defined before publishing
                if (course.CourseAccessType == CourseAccessType.CohortBased && !course.Batches.Any())
                    throw new InvalidOperationException(
                        $"CohortBased course '{id}' cannot be published without at least one batch. Create a batch first.");

                // Validation: Must have at least one section
                if (!course.Sections.Any())
                    throw new InvalidOperationException(
                        "Course must have at least one section before submitting for review.");

                // Validation: Each section must have at least one lesson, and valid quizzes/assignments
                foreach (var section in course.Sections)
                {
                    if (!section.Lessons.Any())
                        throw new InvalidOperationException(
                            $"Section '{section.Title}' has no lessons. Each section must have at least one lesson.");

                    // Validation: Any quiz in the section must have at least one question
                    foreach (var quiz in section.Quizzes)
                    {
                        if (!quiz.Questions.Any())
                            throw new InvalidOperationException(
                                $"Quiz '{quiz.Title}' in section '{section.Title}' has no questions. Add at least one question or remove the quiz.");
                    }

                    // Validation: Any assignment must have valid total marks
                    foreach (var assignment in section.Assignments)
                    {
                        if (assignment.TotalMarks <= 0)
                            throw new InvalidOperationException(
                                $"Assignment '{assignment.Title}' in section '{section.Title}' has invalid marks (must be > 0).");
                    }
                }

                course.Status = CourseStatus.PendingApproval;

                await _courseRepository.UpdateAsync(course);

                // Bug #2 + #4 + #5: Invalidate both cache keys and global stats
                await InvalidateCourseCache(id, course.slug);
                await _cacheService.InvalidateAsync(CacheKeyStatsPrefix + "global");

                _logger.LogInformation("Course submitted for approval: Id={Id}", id);
                return _mapper.Map<CourseResponse>(course);
            }
            else
            {
                var course = await _courseRepository.GetCourseWithDetailsAsync(id)
                    ?? throw new KeyNotFoundException($"Course with id '{id}' not found.");

                if (course.Status != CourseStatus.Published && course.Status != CourseStatus.PendingApproval)
                    throw new InvalidOperationException($"Course with id '{id}' is not published or pending approval.");

                var hasNonExpired = await _enrollmentRepository.HasNonExpiredEnrollmentsByCourseAsync(id);
                if (hasNonExpired)
                    throw new InvalidOperationException("Course cannot be unpublished because learners are currently enrolled.");

                course.Status = CourseStatus.Draft;

                if (course.CourseAccessType == CourseAccessType.SelfPaced)
                {
                    foreach (var section in course.Sections)
                    {
                        section.Status = PublishStatus.Draft;
                        foreach (var lesson in section.Lessons)
                        {
                            lesson.Status = PublishStatus.Draft;
                            foreach (var resource in lesson.Resources)
                            {
                                resource.Status = PublishStatus.Draft;
                            }
                        }
                        foreach (var quiz in section.Quizzes) quiz.Status = PublishStatus.Draft;
                        foreach (var assignment in section.Assignments) assignment.Status = PublishStatus.Draft;
                    }
                }

                await _courseRepository.UpdateAsync(course);

                // Bug #2 + #4 + #5: Invalidate both cache keys and global stats
                await InvalidateCourseCache(id, course.slug);
                await _cacheService.InvalidateAsync(CacheKeyStatsPrefix + "global");

                _logger.LogInformation("Course unpublished/cancelled: Id={Id}", id);
                
                var instructor = await _userRepository.GetByIdAsync(course.InstructorId);
                var html = EmailTemplate.GetCourseStatusUpdatedTemplate(
                    instructor.UserProfile?.FirstName ?? instructor.Email,
                    course.Title, "Unpublished", null);
                Message msg = new EmailMessage(instructor.Email, $"Your course '{course.Title}' has been unpublished", html) { IsHtml = true };
                await _notificationService.Send(msg);

                try
                {
                    await _userNotificationsService.CreateAndSendNotificationAsync(
                        userId: course.InstructorId,
                        title: "Course Unpublished",
                        message: $"Your course '{course.Title}' has been unpublished.",
                        type: NotificationType.General,
                        redirectUrl: $"/courses/{course.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send course unpublished realtime notification to Instructor {InstructorId}", course.InstructorId);
                }

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

                // Bug #2 + #4 + #5: Invalidate both cache keys and global stats
                await InvalidateCourseCache(id, course.slug);
                await _cacheService.InvalidateAsync(CacheKeyStatsPrefix + "global");

                _logger.LogInformation("Course approved and published: Id={Id}", id);

                var instructor = await _userRepository.GetByIdAsync(course.InstructorId);
                var html = EmailTemplate.GetCourseStatusUpdatedTemplate(
                    instructor.UserProfile?.FirstName ?? instructor.Email,
                    course.Title, "Published", null);
                Message msg = new EmailMessage(instructor.Email, $"Your course '{course.Title}' has been published!", html) { IsHtml = true };
                await _notificationService.Send(msg);

                try
                {
                    await _userNotificationsService.CreateAndSendNotificationAsync(
                        userId: course.InstructorId,
                        title: "Course Published",
                        message: $"Congratulations! Your course '{course.Title}' has been approved and published.",
                        type: NotificationType.CoursePublished,
                        redirectUrl: $"/courses/{course.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send course published realtime notification to Instructor {InstructorId}", course.InstructorId);
                }

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

                // Bug #2 + #4: Invalidate both cache keys
                await InvalidateCourseCache(id, course.slug);

                _logger.LogInformation("Course rejected: Id={Id}, Reason={Reason}", id, request.Reason);

                var instructor = await _userRepository.GetByIdAsync(course.InstructorId);
                var html = EmailTemplate.GetCourseStatusUpdatedTemplate(
                    instructor.UserProfile?.FirstName ?? instructor.Email,
                    course.Title, "Rejected", request.Reason);
                Message msg = new EmailMessage(instructor.Email, $"Your course '{course.Title}' was not approved", html) { IsHtml = true };
                await _notificationService.Send(msg);

                try
                {
                    await _userNotificationsService.CreateAndSendNotificationAsync(
                        userId: course.InstructorId,
                        title: "Course Rejected",
                        message: $"Your course '{course.Title}' was not approved. Reason: {request.Reason}",
                        type: NotificationType.General,
                        redirectUrl: $"/courses/{course.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send course rejected realtime notification to Instructor {InstructorId}", course.InstructorId);
                }

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

        public async Task<PagedCourseListResponse> GetPendingCoursesPagedAsync(CourseSearchQuery query)
        {
            var (courses, totalCount) = await _courseRepository.GetPendingCoursesPagedAsync(query);

            var courseList = _mapper.Map<IEnumerable<CourseListItemResponse>>(courses).ToList();
            
            var statsDict = await _courseRepository.GetRatingStatsBatchAsync(courseList.Select(c => c.Id));
            foreach (var c in courseList)
            {
                if (statsDict.TryGetValue(c.Id, out var stats))
                {
                    c.AverageRating = stats.AverageRating;
                    c.TotalReviews = stats.TotalReviews;
                }
            }

            var totalPages = (int)System.Math.Ceiling((double)totalCount / query.PageSize);

            return new PagedCourseListResponse
            {
                Courses = courseList,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = totalPages
            };
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

        public async Task<PagedInstructorCourseResponse> GetCoursesByInstructorPagedAsync(int instructorId, CourseSearchQuery query)
        {
            var (courses, totalCount) = await _courseRepository.GetCoursesByInstructorPagedAsync(instructorId, query);
            var courseList = courses == null ? new List<InstructorCourseCardResponse>() : _mapper.Map<IEnumerable<InstructorCourseCardResponse>>(courses).ToList();

            if (courseList.Any())
            {
                var statsDict = await _courseRepository.GetRatingStatsBatchAsync(courseList.Select(c => c.Id));
                foreach (var c in courseList)
                {
                    if (statsDict.TryGetValue(c.Id, out var stats))
                    {
                        c.AverageRating = stats.AverageRating;
                        c.TotalReviews = stats.TotalReviews;
                    }
                    c.HasNonExpiredEnrollments = await _enrollmentRepository.HasNonExpiredEnrollmentsByCourseAsync(c.Id);
                    c.HasActiveEnrollments = await _enrollmentRepository.HasActiveOnlyEnrollmentsByCourseAsync(c.Id);
                }
            }

            var totalPages = (int)System.Math.Ceiling((double)totalCount / query.PageSize);

            return new PagedInstructorCourseResponse
            {
                Courses = courseList,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = totalPages
            };
        }

        public async Task<IEnumerable<CourseResponse>> GetCoursesByCategoryAsync(int categoryId)
        {
            var courses = await _courseRepository.GetCoursesByCategoryAsync(categoryId);
            var responses = courses == null ? new List<CourseResponse>() : _mapper.Map<IEnumerable<CourseResponse>>(courses).ToList();
            await PopulateRatingStatsListAsync(responses);
            return responses;
        }

        public async Task<CourseSummaryStatsResponse> GetCourseSummaryStatsAsync()
        {
            return await _cacheService.GetOrSetAsync(
                CacheKeyStatsPrefix + "global",
                async () => await _courseRepository.GetCourseSummaryStatsAsync(),
                TimeSpan.FromMinutes(_statsTtlMinutes));
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

        /// <summary>
        /// Invalidates the detail (by-id) and slug cache entries for a course.
        /// Always call this after any write that changes course state (Bug #2, #4).
        /// </summary>
        private async Task InvalidateCourseCache(int courseId, string? slug)
        {
            var keysToInvalidate = new System.Collections.Generic.List<string>
            {
                $"{CacheKeyDetailPrefix}{courseId}"
            };
            if (!string.IsNullOrEmpty(slug))
            {
                keysToInvalidate.Add($"{CacheKeySlugPrefix}{slug}");
            }
            await _cacheService.InvalidateAsync(keysToInvalidate.ToArray());
        }

        private void PopulateEnrollmentStatsList(IEnumerable<CourseResponse> courseList, IEnumerable<Courses> originalCourses)
        {
            foreach (var response in courseList)
            {
                var original = originalCourses.FirstOrDefault(c => c.Id == response.Id);
                if (original != null)
                {
                    response.EnrolledCount = original.Enrollments?.Count ?? 0;
                    response.CompletionRate = original.Enrollments != null && original.Enrollments.Count > 0 
                        ? (double)original.Enrollments.Count(e => e.IsCompleted) / original.Enrollments.Count * 100 
                        : 0;
                }
            }
        }

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
