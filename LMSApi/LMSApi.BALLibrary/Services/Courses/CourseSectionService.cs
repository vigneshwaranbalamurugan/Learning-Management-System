using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;

namespace LMSApi.BALLibrary.Services.Courses
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
            // Validate parent course exists
            await _courseRepository.GetByIdAsync(request.CourseId);

            var section = _mapper.Map<CourseSection>(request);
            await _sectionRepository.AddAsync(section);

            _logger.LogInformation("Section Created: '{Title}' for CourseId={CourseId}", request.Title, request.CourseId);

            return _mapper.Map<SectionResponse>(section);
        }

        public async Task<SectionResponse> UpdateSectionAsync(int id, UpdateSectionRequest request)
        {
            var section = await _sectionRepository.GetByIdAsync(id);

            if (request.Title != null) section.Title = request.Title;
            if (request.Description != null) section.Description = request.Description;
            if (request.TimeLimitMinutes.HasValue) section.TimeLimitMinutes = request.TimeLimitMinutes.Value;
            if (request.TotalMarks.HasValue) section.TotalMarks = request.TotalMarks.Value;
            if (request.PassingMarks.HasValue) section.PassingMarks = request.PassingMarks.Value;
            if (request.MaxAttempts.HasValue) section.MaxAttempts = request.MaxAttempts.Value;
            if (request.IsPublished.HasValue) section.IsPublished = request.IsPublished.Value;

            await _sectionRepository.UpdateAsync(section);

            _logger.LogInformation("Section Updated: Id={Id}", id);

            return _mapper.Map<SectionResponse>(section);
        }

        public async Task DeleteSectionAsync(int id)
        {
            await _sectionRepository.GetByIdAsync(id);
            await _sectionRepository.DeleteAsync(id);

            _logger.LogInformation("Section Deleted: Id={Id}", id);
        }
    }
}
