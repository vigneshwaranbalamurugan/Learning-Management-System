using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;

namespace LMSApi.BALLibrary.Services
{
    public class LessonResourceService : ILessonResourceService
    {
        private readonly ILessonResourceRepository _resourceRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<LessonResourceService> _logger;

        public LessonResourceService(
            ILessonResourceRepository resourceRepository,
            ILessonRepository lessonRepository,
            IMapper mapper,
            ILogger<LessonResourceService> logger)
        {
            _resourceRepository = resourceRepository;
            _lessonRepository = lessonRepository;
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

        public async Task<ResourceResponse> AddResourceAsync(CreateResourceRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.ResourceTitle)) throw new ArgumentException("Resource title cannot be null or empty.", nameof(request.ResourceTitle));
            if (string.IsNullOrWhiteSpace(request.ResourceUrl)) throw new ArgumentException("Resource URL cannot be null or empty.", nameof(request.ResourceUrl));

            // Validate parent lesson exists
            await _lessonRepository.GetByIdAsync(request.LessonId);

            var resource = _mapper.Map<LessonResources>(request);
            resource.UploadedAt = DateTime.UtcNow;

            await _resourceRepository.AddAsync(resource);

            _logger.LogInformation("Resource Uploaded: '{Title}' for LessonId={LessonId}", request.ResourceTitle, request.LessonId);

            return _mapper.Map<ResourceResponse>(resource);
        }

        public async Task<ResourceResponse> UpdateResourceAsync(int id, UpdateResourceRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var resource = await _resourceRepository.GetByIdAsync(id);

            if (request.ResourceType.HasValue) resource.ResourceType = request.ResourceType.Value;
            if (request.ResourceTitle != null) resource.ResourceTitle = request.ResourceTitle;
            if (request.ResourceUrl != null) resource.ResourceUrl = request.ResourceUrl;
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
