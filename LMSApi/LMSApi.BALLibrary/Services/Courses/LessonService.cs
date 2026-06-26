using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.Enums;
using Microsoft.Extensions.Logging;

namespace LMSApi.BALLibrary.Services
{
    public class LessonService : ILessonService
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseSectionRepository _sectionRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUploadService _uploadService;
        private readonly IMapper _mapper;
        private readonly ILogger<LessonService> _logger;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly INotificationService _notificationService;
        private readonly IUserNotificationsService _userNotificationsService;

        public LessonService(
            ILessonRepository lessonRepository,
            ICourseSectionRepository sectionRepository,
            ICourseRepository courseRepository,
            IUploadService uploadService,
            IMapper mapper,
            ILogger<LessonService> logger,
            IEnrollmentRepository enrollmentRepository,
            INotificationService notificationService,
            IUserNotificationsService userNotificationsService)
        {
            _lessonRepository = lessonRepository;
            _sectionRepository = sectionRepository;
            _courseRepository = courseRepository;
            _uploadService = uploadService;
            _mapper = mapper;
            _logger = logger;
            _enrollmentRepository = enrollmentRepository;
            _notificationService = notificationService;
            _userNotificationsService = userNotificationsService;
        }

        public async Task<IEnumerable<LessonResponse>> GetLessonsBySectionAsync(int sectionId, int? currentUserId = null, bool isAdmin = false)
        {
            var section = await _sectionRepository.GetByIdAsync(sectionId)
                ?? throw new KeyNotFoundException($"Section with id '{sectionId}' not found.");

            var course = await _courseRepository.GetByIdAsync(section.CourseId)
                ?? throw new KeyNotFoundException($"Course with id '{section.CourseId}' not found.");

            var lessons = await _lessonRepository.GetLessonsBySectionAsync(sectionId);

            if (currentUserId == null || (course.InstructorId != currentUserId && !isAdmin))
            {
                lessons = lessons.Where(l => l.Status == PublishStatus.Published);
            }

            return _mapper.Map<IEnumerable<LessonResponse>>(lessons);
        }

        public async Task<LessonResponse> GetLessonByIdAsync(int id, int? currentUserId = null, bool isAdmin = false)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Lesson with id '{id}' not found.");

            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId)
                ?? throw new KeyNotFoundException($"Section with id '{lesson.CourseSectionId}' not found.");

            var course = await _courseRepository.GetByIdAsync(section.CourseId)
                ?? throw new KeyNotFoundException($"Course with id '{section.CourseId}' not found.");

            if (currentUserId == null || (course.InstructorId != currentUserId && !isAdmin))
            {
                if (lesson.Status != PublishStatus.Published)
                {
                    throw new KeyNotFoundException($"Lesson with id '{id}' not found.");
                }
            }

            return _mapper.Map<LessonResponse>(lesson);
        }

        public async Task<LessonDetailResponse> GetLessonDetailAsync(int id, int? currentUserId = null, bool isAdmin = false)
        {
            var lesson = await _lessonRepository.GetLessonWithResourcesAsync(id)
                ?? throw new KeyNotFoundException($"Lesson with id '{id}' not found.");

            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId)
                ?? throw new KeyNotFoundException($"Section with id '{lesson.CourseSectionId}' not found.");

            var course = await _courseRepository.GetByIdAsync(section.CourseId)
                ?? throw new KeyNotFoundException($"Course with id '{section.CourseId}' not found.");

            if (currentUserId == null || (course.InstructorId != currentUserId && !isAdmin))
            {
                if (lesson.Status != PublishStatus.Published)
                {
                    throw new KeyNotFoundException($"Lesson with id '{id}' not found.");
                }
                lesson.Resources = lesson.Resources.Where(r => r.Status == PublishStatus.Published).ToList();
            }

            return _mapper.Map<LessonDetailResponse>(lesson);
        }

        public async Task<LessonResponse> CreateLessonAsync(CreateLessonRequest request, Stream? fileStream = null, string? fileName = null)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Title)) throw new ArgumentException("Lesson title cannot be null or empty.", nameof(request.Title));

            // Validate parent section exists
            var section = await _sectionRepository.GetByIdAsync(request.CourseSectionId);

            var course = await _courseRepository.GetByIdAsync(section.CourseId);

            var courseSections = await _sectionRepository.GetSectionsByCourseAsync(course.Id);
            foreach (var s in courseSections)
            {
                var existingLessonsForSection = await _lessonRepository.GetLessonsBySectionAsync(s.Id);
                if (existingLessonsForSection.Any(l => string.Equals(l.Title.Trim(), request.Title.Trim(), StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException("A lesson with this title already exists in this course.");
                }
            }

            // Auto-assign SortOrder if not provided (default 0)
            if (request.SortOrder == 0)
            {
                var existingLessons = await _lessonRepository.GetLessonsBySectionAsync(request.CourseSectionId);
                request.SortOrder = existingLessons.Any() ? existingLessons.Max(l => l.SortOrder) + 1 : 1;
            }

            var lesson = _mapper.Map<Lessons>(request);
            lesson.Content ??= string.Empty;
            lesson.Description ??= string.Empty;
            lesson.CreatedAt = DateTime.UtcNow;
            lesson.UpdatedAt = DateTime.UtcNow;

            if (course.CourseAccessType == CourseAccessType.SelfPaced && course.Status == CourseStatus.Published)
            {
                lesson.Status = PublishStatus.Published;
            }

            await _lessonRepository.AddAsync(lesson);

            var needsUpdate = false;

            // Upload content file for Video and Pdf types
            if (fileStream != null && fileName != null)
            {
                if (lesson.Type == LessonType.Video)
                {
                    var uploadResult = await _uploadService.UploadLessonVideoAsync(
                        fileStream, fileName, $"lessons/{lesson.Id}/video");
                    lesson.ContentUrl = uploadResult.Url;
                    lesson.DurationInMinutes = TimeSpan.FromSeconds(uploadResult.DurationSeconds);
                    needsUpdate = true;
                    _logger.LogInformation("Lesson video uploaded on create: LessonId={LessonId}", lesson.Id);
                }
                else if (lesson.Type == LessonType.Pdf)
                {
                    lesson.ContentUrl = await _uploadService.UploadLessonPdfAsync(
                        fileStream, fileName, $"lessons/{lesson.Id}/pdf");
                    needsUpdate = true;
                    _logger.LogInformation("Lesson PDF uploaded on create: LessonId={LessonId}", lesson.Id);
                }
            }

            if (needsUpdate)
            {
                await _lessonRepository.UpdateAsync(lesson);
            }

            _logger.LogInformation("Lesson Created: '{Title}' for SectionId={SectionId}", request.Title, request.CourseSectionId);

            await _courseRepository.UpdateCourseDurationAsync(course.Id);

            return _mapper.Map<LessonResponse>(lesson);
        }

        public async Task<LessonResponse> UpdateLessonAsync(int id, UpdateLessonRequest request, Stream? fileStream = null, string? fileName = null)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var lesson = await _lessonRepository.GetByIdAsync(id);

            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);

            if (request.CourseSectionId.HasValue && request.CourseSectionId.Value != lesson.CourseSectionId)
            {
                var newSection = await _sectionRepository.GetByIdAsync(request.CourseSectionId.Value);
                if (newSection == null || newSection.CourseId != course.Id)
                {
                    throw new InvalidOperationException("Invalid target section. The section must belong to the same course.");
                }
                lesson.CourseSectionId = request.CourseSectionId.Value;
                
                if (!request.SortOrder.HasValue)
                {
                    var existingLessons = await _lessonRepository.GetLessonsBySectionAsync(newSection.Id);
                    lesson.SortOrder = existingLessons.Any() ? existingLessons.Max(l => l.SortOrder) + 1 : 1;
                }
            }

            if (request.Title != null)
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                    throw new ArgumentException("Lesson title cannot be null or empty.", nameof(request.Title));

                var courseSections = await _sectionRepository.GetSectionsByCourseAsync(course.Id);
                foreach (var s in courseSections)
                {
                    var existingLessonsForSection = await _lessonRepository.GetLessonsBySectionAsync(s.Id);
                    if (existingLessonsForSection.Any(l => l.Id != id && string.Equals(l.Title.Trim(), request.Title.Trim(), StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new InvalidOperationException("A lesson with this title already exists in this course.");
                    }
                }
                lesson.Title = request.Title;
            }
            if (request.Description != null) lesson.Description = request.Description;
            if (request.Content != null) lesson.Content = request.Content;
            if (request.ContentUrl != null) lesson.ContentUrl = request.ContentUrl;
            if (request.Type.HasValue) lesson.Type = request.Type.Value;
            if (request.DurationInMinutes.HasValue) lesson.DurationInMinutes = request.DurationInMinutes.Value;
            if (request.SortOrder.HasValue) lesson.SortOrder = request.SortOrder.Value;
            if (request.IsPreview.HasValue) lesson.IsPreview = request.IsPreview.Value;
            if (request.Status.HasValue) 
            {
                if (course.CourseAccessType == CourseAccessType.SelfPaced)
                {
                    throw new InvalidOperationException("Cannot manually change publish status of a lesson in a Self-Paced course.");
                }
                lesson.Status = request.Status.Value;
            }

            if (course.CourseAccessType == CourseAccessType.SelfPaced && course.Status == CourseStatus.Published)
            {
                lesson.Status = PublishStatus.Published;
            }

            lesson.UpdatedAt = DateTime.UtcNow;

            // Upload replacement content file for Video and Pdf types
            if (fileStream != null && fileName != null)
            {
                if (lesson.Type == LessonType.Video)
                {
                    var uploadResult = await _uploadService.UploadLessonVideoAsync(
                        fileStream, fileName, $"lessons/{lesson.Id}/video");
                    lesson.ContentUrl = uploadResult.Url;
                    lesson.DurationInMinutes = TimeSpan.FromSeconds(uploadResult.DurationSeconds);
                    _logger.LogInformation("Lesson video uploaded on update: LessonId={LessonId}", lesson.Id);
                }
                else if (lesson.Type == LessonType.Pdf)
                {
                    lesson.ContentUrl = await _uploadService.UploadLessonPdfAsync(
                        fileStream, fileName, $"lessons/{lesson.Id}/pdf");
                    _logger.LogInformation("Lesson PDF uploaded on update: LessonId={LessonId}", lesson.Id);
                }
            }

            await _lessonRepository.UpdateAsync(lesson);

            await _courseRepository.UpdateCourseDurationAsync(course.Id);

            _logger.LogInformation("Lesson Updated: Id={Id}", id);

            return _mapper.Map<LessonResponse>(lesson);
        }

        public async Task DeleteLessonAsync(int id)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);

            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);
            await _lessonRepository.DeleteAsync(id);

            await _courseRepository.UpdateCourseDurationAsync(course.Id);

            _logger.LogInformation("Lesson Deleted: Id={Id}", id);
        }

        public async Task ReorderLessonsAsync(ReorderLessonsRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.LessonOrders == null) throw new ArgumentException("Lesson orders list cannot be null.", nameof(request.LessonOrders));

            foreach (var item in request.LessonOrders)
            {
                var lesson = await _lessonRepository.GetByIdAsync(item.LessonId);
                lesson.SortOrder = item.SortOrder;
                await _lessonRepository.UpdateAsync(lesson);
            }

            _logger.LogInformation("Lessons Reordered: {Count} lessons updated", request.LessonOrders.Count);
        }

        public async Task<LessonResponse> PublishLessonAsync(int id, PublishLessonRequest request)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);
            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);

            if (course.CourseAccessType == CourseAccessType.SelfPaced)
            {
                throw new InvalidOperationException("Cannot manually change publish status of a lesson in a Self-Paced course.");
            }

            lesson.Status = request.Publish ? PublishStatus.Published : PublishStatus.Draft;
            await _lessonRepository.UpdateAsync(lesson);

            _logger.LogInformation("Lesson publication status updated: LessonId={LessonId}, Status={Status}", id, lesson.Status);

            if (request.Publish && course.CourseAccessType == CourseAccessType.CohortBased)
            {
                var enrollments = await _enrollmentRepository.GetActiveEnrollmentsByCourseAsync(course.Id);
                var emailsToSend = enrollments.Select(e => new
                {
                    UserId = e.UserId,
                    Email = e.User.Email,
                    Name = e.User.UserProfile?.FirstName ?? e.User.Email,
                    BatchName = e.Batch?.Name ?? ""
                }).ToList();

                var courseTitle = course.Title;
                var lessonTitle = lesson.Title;

                _ = Task.Run(async () =>
                {
                    foreach (var e in emailsToSend)
                    {
                        var html = Utils.EmailTemplate.GetContentPublishedTemplate(
                            e.Name, courseTitle, "Lesson", lessonTitle, e.BatchName);
                        Message msg = new EmailMessage(e.Email, $"New lesson available: {lessonTitle}", html) { IsHtml = true };
                        try
                        {
                            await _notificationService.Send(msg);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send lesson published email to {Email}", e.Email);
                        }

                        try
                        {
                            await _userNotificationsService.CreateAndSendNotificationAsync(
                                userId: e.UserId,
                                title: "New Lesson Published",
                                message: $"A new lesson '{lessonTitle}' has been published in '{courseTitle}'.",
                                type: NotificationType.General,
                                redirectUrl: $"/courses/{course.Id}/lessons/{lesson.Id}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send lesson published realtime notification to User {UserId}", e.UserId);
                        }
                    }
                });
            }

            return _mapper.Map<LessonResponse>(lesson);
        }
    }
}
