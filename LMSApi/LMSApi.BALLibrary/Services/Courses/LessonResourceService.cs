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

        public LessonResourceService(
            ILessonResourceRepository resourceRepository,
            ILessonRepository lessonRepository,
            ICourseSectionRepository sectionRepository,
            ICourseRepository courseRepository,
            IUploadService uploadService,
            IMapper mapper,
            ILogger<LessonResourceService> logger)
        {
            _resourceRepository = resourceRepository;
            _lessonRepository = lessonRepository;
            _sectionRepository = sectionRepository;
            _courseRepository = courseRepository;
            _uploadService = uploadService;
            _mapper = mapper;
            _logger = logger;
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

            await _resourceRepository.AddAsync(resource);

            _logger.LogInformation("Resource Uploaded: '{Title}' for LessonId={LessonId}", request.ResourceTitle, request.LessonId);

            return _mapper.Map<ResourceResponse>(resource);
        }

        public async Task<ResourceResponse> UpdateResourceAsync(int id, UpdateResourceRequest request, System.IO.Stream? fileStream = null, string? fileName = null)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var resource = await _resourceRepository.GetByIdAsync(id);

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

            var lesson = await _lessonRepository.GetByIdAsync(resource.LessonId);
            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);

            if (request.ResourceType.HasValue) resource.ResourceType = request.ResourceType.Value;
            if (request.ResourceTitle != null) resource.ResourceTitle = request.ResourceTitle;
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
            await _resourceRepository.GetByIdAsync(id);
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

            return _mapper.Map<ResourceResponse>(resource);
        }
    }
}
