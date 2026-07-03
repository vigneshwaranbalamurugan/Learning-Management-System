using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.Enums;
using Microsoft.Extensions.Logging;

namespace LMSApi.BALLibrary.Services
{
    public class LessonResourceService : ILessonResourceService
    {
        private readonly ILessonResourceRepository _resourceRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseSectionRepository _sectionRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUploadService _uploadService;
        private readonly IMapper _mapper;
        private readonly ILogger<LessonResourceService> _logger;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly INotificationService _notificationService;
        private readonly IUserNotificationsService _userNotificationsService;

        public LessonResourceService(
            ILessonResourceRepository resourceRepository,
            ILessonRepository lessonRepository,
            ICourseSectionRepository sectionRepository,
            ICourseRepository courseRepository,
            IUploadService uploadService,
            IMapper mapper,
            ILogger<LessonResourceService> logger,
            IEnrollmentRepository enrollmentRepository,
            INotificationService notificationService,
            IUserNotificationsService userNotificationsService)
        {
            _resourceRepository = resourceRepository;
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

        public async Task<IEnumerable<ResourceResponse>> GetResourcesByLessonAsync(int lessonId, int? currentUserId = null, bool isAdmin = false)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId)
                ?? throw new KeyNotFoundException($"Lesson with id '{lessonId}' not found.");

            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId)
                ?? throw new KeyNotFoundException($"Section with id '{lesson.CourseSectionId}' not found.");

            var course = await _courseRepository.GetByIdAsync(section.CourseId)
                ?? throw new KeyNotFoundException($"Course with id '{section.CourseId}' not found.");

            var resources = await _resourceRepository.GetResourcesByLessonAsync(lessonId);

            if (currentUserId == null || (course.InstructorId != currentUserId && !isAdmin))
            {
                bool isEnrolled = false;
                if (currentUserId.HasValue)
                {
                    isEnrolled = await _enrollmentRepository.IsAlreadyEnrolledAsync(currentUserId.Value, course.Id);
                }

                if (!isEnrolled)
                {
                    throw new KeyNotFoundException($"Lesson with id '{lessonId}' not found.");
                }

                resources = resources.Where(r => r.Status == PublishStatus.Published);
            }

            return _mapper.Map<IEnumerable<ResourceResponse>>(resources);
        }

        public async Task<ResourceResponse> GetResourceByIdAsync(int id, int? currentUserId = null, bool isAdmin = false)
        {
            var resource = await _resourceRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"Resource with id '{id}' not found.");

            var lesson = await _lessonRepository.GetByIdAsync(resource.LessonId)
                ?? throw new KeyNotFoundException($"Lesson with id '{resource.LessonId}' not found.");

            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId)
                ?? throw new KeyNotFoundException($"Section with id '{lesson.CourseSectionId}' not found.");

            var course = await _courseRepository.GetByIdAsync(section.CourseId)
                ?? throw new KeyNotFoundException($"Course with id '{section.CourseId}' not found.");

            if (currentUserId == null || (course.InstructorId != currentUserId && !isAdmin))
            {
                if (resource.Status != PublishStatus.Published)
                {
                    throw new KeyNotFoundException($"Resource with id '{id}' not found.");
                }

                bool isEnrolled = false;
                if (currentUserId.HasValue)
                {
                    isEnrolled = await _enrollmentRepository.IsAlreadyEnrolledAsync(currentUserId.Value, course.Id);
                }

                if (!isEnrolled)
                {
                    throw new KeyNotFoundException($"Resource with id '{id}' not found.");
                }
            }

            return _mapper.Map<ResourceResponse>(resource);
        }

        public async Task<ResourceResponse> AddResourceAsync(CreateResourceRequest request, System.IO.Stream? fileStream = null, string? fileName = null)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.ResourceTitle)) throw new ArgumentException("Resource title cannot be null or empty.", nameof(request.ResourceTitle));

            // Validate parent lesson exists
            var lesson = await _lessonRepository.GetByIdAsync(request.LessonId);
            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);

            var hasNonExpired = await _enrollmentRepository.HasNonExpiredEnrollmentsByCourseAsync(course.Id);
            if (hasNonExpired)
            {
                throw new InvalidOperationException("Cannot add a resource to a course that has enrolled learners.");
            }

            var courseSections = await _sectionRepository.GetSectionsByCourseAsync(course.Id);
            foreach (var s in courseSections)
            {
                var sectionLessons = await _lessonRepository.GetLessonsBySectionAsync(s.Id);
                foreach (var l in sectionLessons)
                {
                    var existingResources = await _resourceRepository.GetResourcesByLessonAsync(l.Id);
                    if (existingResources.Any(r => string.Equals(r.ResourceTitle.Trim(), request.ResourceTitle.Trim(), StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new InvalidOperationException("A lesson resource with this title already exists in this course.");
                    }
                }
            }

            string resourceUrl;
            if (request.ResourceType == ResourceType.ExternalLink)
            {
                if (string.IsNullOrWhiteSpace(request.ResourceUrl))
                    throw new ArgumentException("Resource URL is required for External Link resource type.", nameof(request.ResourceUrl));
                if (fileStream != null)
                    throw new ArgumentException("Cannot upload a file for an External Link resource type.");
                resourceUrl = request.ResourceUrl;
            }
            else if (request.ResourceType == ResourceType.Pdf)
            {
                if (fileStream == null || fileName == null)
                    throw new ArgumentException("A PDF file must be uploaded for PDF resource type.");

                // Upload to Cloudinary with a unique path
                var uniqueId = Guid.NewGuid().ToString();
                resourceUrl = await _uploadService.UploadLessonPdfAsync(
                    fileStream, fileName, $"lessons/{request.LessonId}/resources/{uniqueId}");
            }
            else
            {
                throw new ArgumentException("Invalid resource type.");
            }

            var resource = _mapper.Map<LessonResources>(request);
            resource.ResourceUrl = resourceUrl;
            resource.UploadedAt = DateTime.UtcNow;

            if (course.CourseAccessType == CourseAccessType.SelfPaced)
            {
                if (course.Status == CourseStatus.Published)
                {
                    resource.Status = PublishStatus.Published;
                }
                else
                {
                    resource.Status = PublishStatus.Draft;
                }
            }
            else
            {
                resource.Status = request.Status;
            }

            var currentResources = await _resourceRepository.GetResourcesByLessonAsync(request.LessonId);
            resource.SortOrder = currentResources.Any() ? currentResources.Max(r => r.SortOrder) + 1 : 1;

            await _resourceRepository.AddAsync(resource);

            _logger.LogInformation("Resource Uploaded: '{Title}' for LessonId={LessonId}", request.ResourceTitle, request.LessonId);

            return _mapper.Map<ResourceResponse>(resource);
        }

        public async Task<ResourceResponse> UpdateResourceAsync(int id, UpdateResourceRequest request, System.IO.Stream? fileStream = null, string? fileName = null)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var resource = await _resourceRepository.GetByIdAsync(id);

            var lesson = await _lessonRepository.GetByIdAsync(resource.LessonId);
            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);

            var hasNonExpired = await _enrollmentRepository.HasNonExpiredEnrollmentsByCourseAsync(course.Id);
            if (hasNonExpired)
            {
                if (request.ResourceType.HasValue || request.ResourceUrl != null || fileStream != null)
                {
                    throw new InvalidOperationException("Cannot update resource files or links because the course has enrolled learners.");
                }
            }

            var finalType = request.ResourceType ?? resource.ResourceType;

            if (finalType == ResourceType.ExternalLink)
            {
                if (request.ResourceUrl != null)
                {
                    if (string.IsNullOrWhiteSpace(request.ResourceUrl))
                        throw new ArgumentException("Resource URL cannot be empty for external links.");
                    resource.ResourceUrl = request.ResourceUrl;
                }
                else if (resource.ResourceType != ResourceType.ExternalLink)
                {
                    // If changing from Pdf to ExternalLink, a URL must be provided in the request
                    throw new ArgumentException("Resource URL is required when changing resource type to External Link.");
                }

                if (fileStream != null)
                    throw new ArgumentException("Cannot upload a file for an External Link resource type.");
            }
            else if (finalType == ResourceType.Pdf)
            {
                if (fileStream != null && fileName != null)
                {
                    // Upload new PDF to Cloudinary
                    resource.ResourceUrl = await _uploadService.UploadLessonPdfAsync(
                        fileStream, fileName, $"lessons/{resource.LessonId}/resources/{resource.Id}");
                }
                else if (resource.ResourceType != ResourceType.Pdf)
                {
                    // If changing from ExternalLink to Pdf, a PDF file must be provided
                    throw new ArgumentException("A PDF file must be uploaded when changing resource type to PDF.");
                }
            }

            if (request.ResourceType.HasValue) resource.ResourceType = request.ResourceType.Value;
            
            if (request.ResourceTitle != null)
            {
                if (string.IsNullOrWhiteSpace(request.ResourceTitle))
                    throw new ArgumentException("Resource title cannot be null or empty.", nameof(request.ResourceTitle));

                var courseSections = await _sectionRepository.GetSectionsByCourseAsync(course.Id);
                foreach (var s in courseSections)
                {
                    var sectionLessons = await _lessonRepository.GetLessonsBySectionAsync(s.Id);
                    foreach (var l in sectionLessons)
                    {
                        var existingResources = await _resourceRepository.GetResourcesByLessonAsync(l.Id);
                        if (existingResources.Any(r => r.Id != id && string.Equals(r.ResourceTitle.Trim(), request.ResourceTitle.Trim(), StringComparison.OrdinalIgnoreCase)))
                        {
                            throw new InvalidOperationException("A lesson resource with this title already exists in this course.");
                        }
                    }
                }
                resource.ResourceTitle = request.ResourceTitle;
            }
            if (request.Description != null) resource.Description = request.Description;
            if (request.Status.HasValue)
            {
                if (course.CourseAccessType == CourseAccessType.SelfPaced)
                {
                    throw new InvalidOperationException("Cannot manually change publish status of a resource in a Self-Paced course.");
                }
                resource.Status = request.Status.Value;
            }

            if (course.CourseAccessType == CourseAccessType.SelfPaced && course.Status == CourseStatus.Published)
            {
                resource.Status = PublishStatus.Published;
            }

            await _resourceRepository.UpdateAsync(resource);

            _logger.LogInformation("Resource Updated: Id={Id}", id);

            return _mapper.Map<ResourceResponse>(resource);
        }

        public async Task DeleteResourceAsync(int id)
        {
            var resource = await _resourceRepository.GetByIdAsync(id);
            var lesson = await _lessonRepository.GetByIdAsync(resource.LessonId);
            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);

            var hasNonExpired = await _enrollmentRepository.HasNonExpiredEnrollmentsByCourseAsync(course.Id);
            if (hasNonExpired)
            {
                throw new InvalidOperationException("Cannot delete a resource from a course that has enrolled learners.");
            }

            await _resourceRepository.DeleteAsync(id);

            _logger.LogInformation("Resource Deleted: Id={Id}", id);
        }

        public async Task<ResourceResponse> PublishResourceAsync(int id, PublishResourceRequest request)
        {
            var resource = await _resourceRepository.GetByIdAsync(id);
            var lesson = await _lessonRepository.GetByIdAsync(resource.LessonId);
            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);

            if (course.CourseAccessType == CourseAccessType.SelfPaced)
            {
                throw new InvalidOperationException("Cannot manually change publish status of a resource in a Self-Paced course.");
            }

            resource.Status = request.Publish ? PublishStatus.Published : PublishStatus.Draft;
            await _resourceRepository.UpdateAsync(resource);

            _logger.LogInformation("Resource publication status updated: ResourceId={ResourceId}, Status={Status}", id, resource.Status);

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
                var resourceTitle = resource.ResourceTitle;

                _ = Task.Run(async () =>
                {
                    foreach (var e in emailsToSend)
                    {
                        var html = Utils.EmailTemplate.GetContentPublishedTemplate(
                            e.Name, courseTitle, "Resource", resourceTitle, e.BatchName);
                        Message msg = new EmailMessage(e.Email, $"New resource available: {resourceTitle}", html) { IsHtml = true };
                        try
                        {
                            await _notificationService.Send(msg);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send resource published email to {Email}", e.Email);
                        }

                        try
                        {
                            await _userNotificationsService.CreateAndSendNotificationAsync(
                                userId: e.UserId,
                                title: "New Resource Published",
                                message: $"A new resource '{resourceTitle}' has been published in '{courseTitle}'.",
                                type: NotificationType.General,
                                redirectUrl: $"/courses/{course.Id}/lessons/{lesson.Id}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send resource published realtime notification to User {UserId}", e.UserId);
                        }
                    }
                });
            }

            return _mapper.Map<ResourceResponse>(resource);
        }

        public async Task ReorderResourcesAsync(int lessonId, ReorderResourcesRequest request)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId)
                ?? throw new KeyNotFoundException($"Lesson with id '{lessonId}' not found.");

            var resources = await _resourceRepository.GetResourcesByLessonAsync(lessonId);

            foreach (var item in request.Resources)
            {
                var resource = resources.FirstOrDefault(r => r.Id == item.ResourceId);
                if (resource != null)
                {
                    resource.SortOrder = item.SortOrder;
                    await _resourceRepository.UpdateAsync(resource);
                }
            }

            _logger.LogInformation("Resources reordered for LessonId={LessonId}", lessonId);
        }
    }
}
