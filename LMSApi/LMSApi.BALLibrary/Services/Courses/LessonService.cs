using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;

namespace LMSApi.BALLibrary.Services.Courses
{
    public class LessonService : ILessonService
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseSectionRepository _sectionRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<LessonService> _logger;

        public LessonService(
            ILessonRepository lessonRepository,
            ICourseSectionRepository sectionRepository,
            IMapper mapper,
            ILogger<LessonService> logger)
        {
            _lessonRepository = lessonRepository;
            _sectionRepository = sectionRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<LessonResponse>> GetLessonsBySectionAsync(int sectionId)
        {
            var lessons = await _lessonRepository.GetLessonsBySectionAsync(sectionId);
            return _mapper.Map<IEnumerable<LessonResponse>>(lessons);
        }

        public async Task<LessonResponse> GetLessonByIdAsync(int id)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);
            return _mapper.Map<LessonResponse>(lesson);
        }

        public async Task<LessonResponse> CreateLessonAsync(CreateLessonRequest request)
        {
            // Validate parent section exists
            await _sectionRepository.GetByIdAsync(request.CourseSectionId);

            var lesson = _mapper.Map<Lessons>(request);
            await _lessonRepository.AddAsync(lesson);

            _logger.LogInformation("Lesson Created: '{Title}' for SectionId={SectionId}", request.Title, request.CourseSectionId);

            return _mapper.Map<LessonResponse>(lesson);
        }

        public async Task<LessonResponse> UpdateLessonAsync(int id, UpdateLessonRequest request)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);

            if (request.Title != null) lesson.Title = request.Title;
            if (request.Description != null) lesson.Description = request.Description;
            if (request.Content != null) lesson.Content = request.Content;
            if (request.ExternalUrl != null) lesson.ExternalUrl = request.ExternalUrl;
            if (request.VideoUrl != null) lesson.VideoUrl = request.VideoUrl;
            if (request.Type.HasValue) lesson.Type = request.Type.Value;
            if (request.DurationInMinutes.HasValue) lesson.DurationInMinutes = request.DurationInMinutes.Value;
            if (request.Duration.HasValue) lesson.Duration = request.Duration.Value;
            if (request.SortOrder.HasValue) lesson.SortOrder = request.SortOrder.Value;

            await _lessonRepository.UpdateAsync(lesson);

            _logger.LogInformation("Lesson Updated: Id={Id}", id);

            return _mapper.Map<LessonResponse>(lesson);
        }

        public async Task DeleteLessonAsync(int id)
        {
            await _lessonRepository.GetByIdAsync(id);
            await _lessonRepository.DeleteAsync(id);

            _logger.LogInformation("Lesson Deleted: Id={Id}", id);
        }

        public async Task ReorderLessonsAsync(ReorderLessonsRequest request)
        {
            foreach (var item in request.LessonOrders)
            {
                var lesson = await _lessonRepository.GetByIdAsync(item.LessonId);
                lesson.SortOrder = item.SortOrder;
                await _lessonRepository.UpdateAsync(lesson);
            }

            _logger.LogInformation("Lessons Reordered: {Count} lessons updated", request.LessonOrders.Count);
        }
    }
}
