using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;
using LMSApi.ModelLibrary.Enums;

namespace LMSApi.BALLibrary.Services
{
    public class CourseSectionService : ICourseSectionService
    {
        private readonly ICourseSectionRepository _sectionRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CourseSectionService> _logger;

        public CourseSectionService(
            ICourseSectionRepository sectionRepository,
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<CourseSectionService> logger)
        {
            _sectionRepository = sectionRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<SectionResponse>> GetSectionsByCourseAsync(int courseId)
        {
            var sections = await _sectionRepository.GetSectionsByCourseAsync(courseId);
            return _mapper.Map<IEnumerable<SectionResponse>>(sections);
        }

        public async Task<SectionResponse> GetSectionByIdAsync(int id)
        {
            var section = await _sectionRepository.GetByIdAsync(id);
            return _mapper.Map<SectionResponse>(section);
        }

        public async Task<SectionResponse> CreateSectionAsync(CreateSectionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Title)) throw new ArgumentException("Section title cannot be null or empty.", nameof(request.Title));

            // Validate parent course exists
            var course = await _courseRepository.GetByIdAsync(request.CourseId);

            // Auto-assign SortOrder if not provided (default 0)
            if (request.SortOrder == 0)
            {
                var existingSections = await _sectionRepository.GetSectionsByCourseAsync(request.CourseId);
                request.SortOrder = existingSections.Any() ? existingSections.Max(s => s.SortOrder) + 1 : 1;
            }

            var section = _mapper.Map<CourseSection>(request);

            if (course.CourseAccessType == CourseAccessType.SelfPaced && course.Status == CourseStatus.Published)
            {
                section.IsPublished = true;
            }

            await _sectionRepository.AddAsync(section);

            _logger.LogInformation("Section Created: '{Title}' for CourseId={CourseId}", request.Title, request.CourseId);

            return _mapper.Map<SectionResponse>(section);
        }

        public async Task<SectionResponse> UpdateSectionAsync(int id, UpdateSectionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var section = await _sectionRepository.GetByIdAsync(id);

            var course = await _courseRepository.GetByIdAsync(section.CourseId);

            if (request.Title != null) section.Title = request.Title;
            if (request.Description != null) section.Description = request.Description;
            if (request.EstimatedDuration.HasValue) section.EstimatedDuration = request.EstimatedDuration.Value;
            if (request.SortOrder.HasValue) section.SortOrder = request.SortOrder.Value;
            if (request.IsPublished.HasValue) 
            {
                if (course.CourseAccessType == CourseAccessType.SelfPaced)
                {
                    throw new InvalidOperationException("Cannot manually change publish status of a section in a Self-Paced course.");
                }
                section.IsPublished = request.IsPublished.Value;
            }

            if (course.CourseAccessType == CourseAccessType.SelfPaced && course.Status == CourseStatus.Published)
            {
                section.IsPublished = true;
            }

            await _sectionRepository.UpdateAsync(section);

            _logger.LogInformation("Section Updated: Id={Id}", id);

            return _mapper.Map<SectionResponse>(section);
        }

        public async Task DeleteSectionAsync(int id)
        {
            var section = await _sectionRepository.GetByIdAsync(id);

            var course = await _courseRepository.GetByIdAsync(section.CourseId);
            await _sectionRepository.DeleteAsync(id);

            _logger.LogInformation("Section Deleted: Id={Id}", id);
        }

        public async Task ReorderSectionsAsync(ReorderSectionsRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.SectionOrders == null) throw new ArgumentException("Section orders list cannot be null.", nameof(request.SectionOrders));

            foreach (var item in request.SectionOrders)
            {
                var section = await _sectionRepository.GetByIdAsync(item.SectionId);
                section.SortOrder = item.SortOrder;
                await _sectionRepository.UpdateAsync(section);
            }

            _logger.LogInformation("Sections Reordered: {Count} sections updated", request.SectionOrders.Count);
        }
    }
}
