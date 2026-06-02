using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.Enums;
using Microsoft.Extensions.Logging;

namespace LMSApi.BALLibrary.Services.Courses
{
    public class LessonService : ILessonService
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseSectionRepository _sectionRepository;
        private readonly IUploadService _uploadService;
        private readonly IMapper _mapper;
        private readonly ILogger<LessonService> _logger;

        public LessonService(
            ILessonRepository lessonRepository,
            ICourseSectionRepository sectionRepository,
            IUploadService uploadService,
            IMapper mapper,
            ILogger<LessonService> logger)
        {
            _lessonRepository = lessonRepository;
            _sectionRepository = sectionRepository;
            _uploadService = uploadService;
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

        public async Task<LessonResponse> CreateLessonAsync(CreateLessonRequest request, Stream? fileStream = null, string? fileName = null)
        {
            // Validate parent section exists
            await _sectionRepository.GetByIdAsync(request.CourseSectionId);

            var lesson = _mapper.Map<Lessons>(request);
            lesson.VideoUrl ??= string.Empty;
            lesson.Content ??= string.Empty;
            lesson.Description ??= string.Empty;

            await _lessonRepository.AddAsync(lesson);

            var needsUpdate = false;

            if (fileStream != null && fileName != null)
            {
                if (lesson.Type == LessonType.Video)
                {
                    lesson.VideoUrl = await _uploadService.UploadLessonVideoAsync(
                        fileStream, fileName, $"lessons/{lesson.Id}/video");
                    needsUpdate = true;
                    _logger.LogInformation("Lesson video uploaded on create: LessonId={LessonId}", lesson.Id);
                }
                else if (lesson.Type == LessonType.Pdf)
                {
                    lesson.ExternalUrl = await _uploadService.UploadLessonPdfAsync(
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

            return _mapper.Map<LessonResponse>(lesson);
        }

        public async Task<LessonResponse> UpdateLessonAsync(int id, UpdateLessonRequest request, Stream? fileStream = null, string? fileName = null)
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

            if (fileStream != null && fileName != null)
            {
                if (lesson.Type == LessonType.Video)
                {
                    lesson.VideoUrl = await _uploadService.UploadLessonVideoAsync(
                        fileStream, fileName, $"lessons/{lesson.Id}/video");
                    _logger.LogInformation("Lesson video uploaded on update: LessonId={LessonId}", lesson.Id);
                }
                else if (lesson.Type == LessonType.Pdf)
                {
                    lesson.ExternalUrl = await _uploadService.UploadLessonPdfAsync(
                        fileStream, fileName, $"lessons/{lesson.Id}/pdf");
                    _logger.LogInformation("Lesson PDF uploaded on update: LessonId={LessonId}", lesson.Id);
                }
            }

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
