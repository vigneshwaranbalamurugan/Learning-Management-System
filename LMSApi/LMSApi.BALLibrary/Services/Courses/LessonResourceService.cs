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
        private readonly IUploadService _uploadService;
        private readonly IMapper _mapper;
        private readonly ILogger<LessonResourceService> _logger;

        public LessonResourceService(
            ILessonResourceRepository resourceRepository,
            ILessonRepository lessonRepository,
            IUploadService uploadService,
            IMapper mapper,
            ILogger<LessonResourceService> logger)
        {
            _resourceRepository = resourceRepository;
            _lessonRepository = lessonRepository;
            _uploadService = uploadService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ResourceResponse>> GetResourcesByLessonAsync(int lessonId)
        {
            var resources = await _resourceRepository.GetResourcesByLessonAsync(lessonId);
            return _mapper.Map<IEnumerable<ResourceResponse>>(resources);
        }

        public async Task<ResourceResponse> GetResourceByIdAsync(int id)
        {
            var resource = await _resourceRepository.GetByIdAsync(id);
            return _mapper.Map<ResourceResponse>(resource);
        }

        public async Task<ResourceResponse> AddResourceAsync(CreateResourceRequest request, System.IO.Stream? fileStream = null, string? fileName = null)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.ResourceTitle)) throw new ArgumentException("Resource title cannot be null or empty.", nameof(request.ResourceTitle));

            // Validate parent lesson exists
            await _lessonRepository.GetByIdAsync(request.LessonId);

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

            if (request.ResourceType.HasValue) resource.ResourceType = request.ResourceType.Value;
            if (request.ResourceTitle != null) resource.ResourceTitle = request.ResourceTitle;
            if (request.Description != null) resource.Description = request.Description;

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
    }
}
