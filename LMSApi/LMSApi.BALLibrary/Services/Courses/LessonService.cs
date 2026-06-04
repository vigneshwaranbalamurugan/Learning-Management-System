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

        public LessonService(
            ILessonRepository lessonRepository,
            ICourseSectionRepository sectionRepository,
            ICourseRepository courseRepository,
            IUploadService uploadService,
            IMapper mapper,
            ILogger<LessonService> logger)
        {
            _lessonRepository = lessonRepository;
            _sectionRepository = sectionRepository;
            _courseRepository = courseRepository;
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

        public async Task<LessonDetailResponse> GetLessonDetailAsync(int id)
        {
            var lesson = await _lessonRepository.GetLessonWithResourcesAsync(id);
            return _mapper.Map<LessonDetailResponse>(lesson);
        }

        public async Task<LessonResponse> CreateLessonAsync(CreateLessonRequest request, Stream? fileStream = null, string? fileName = null)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Title)) throw new ArgumentException("Lesson title cannot be null or empty.", nameof(request.Title));

            // Validate parent section exists
            var section = await _sectionRepository.GetByIdAsync(request.CourseSectionId);

            var course = await _courseRepository.GetByIdAsync(section.CourseId);
            if (course.Status == CourseStatus.Published)
            {
                throw new InvalidOperationException($"Cannot create a lesson for section '{request.CourseSectionId}' because the parent course is already published.");
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

            await _lessonRepository.AddAsync(lesson);

            var needsUpdate = false;

            // Upload content file for Video and Pdf types
            if (fileStream != null && fileName != null)
            {
                if (lesson.Type == LessonType.Video)
                {
                    lesson.ContentUrl = await _uploadService.UploadLessonVideoAsync(
                        fileStream, fileName, $"lessons/{lesson.Id}/video");
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

            return _mapper.Map<LessonResponse>(lesson);
        }

        public async Task<LessonResponse> UpdateLessonAsync(int id, UpdateLessonRequest request, Stream? fileStream = null, string? fileName = null)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var lesson = await _lessonRepository.GetByIdAsync(id);

            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);
            if (course.Status == CourseStatus.Published)
            {
                throw new InvalidOperationException($"Cannot update lesson '{id}' because its parent course is already published.");
            }

            if (request.Title != null) lesson.Title = request.Title;
            if (request.Description != null) lesson.Description = request.Description;
            if (request.Content != null) lesson.Content = request.Content;
            if (request.ContentUrl != null) lesson.ContentUrl = request.ContentUrl;
            if (request.Type.HasValue) lesson.Type = request.Type.Value;
            if (request.DurationInMinutes.HasValue) lesson.DurationInMinutes = request.DurationInMinutes.Value;
            if (request.SortOrder.HasValue) lesson.SortOrder = request.SortOrder.Value;
            if (request.IsPreview.HasValue) lesson.IsPreview = request.IsPreview.Value;
            if (request.IsPublished.HasValue) lesson.IsPublished = request.IsPublished.Value;

            lesson.UpdatedAt = DateTime.UtcNow;

            // Upload replacement content file for Video and Pdf types
            if (fileStream != null && fileName != null)
            {
                if (lesson.Type == LessonType.Video)
                {
                    lesson.ContentUrl = await _uploadService.UploadLessonVideoAsync(
                        fileStream, fileName, $"lessons/{lesson.Id}/video");
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

            _logger.LogInformation("Lesson Updated: Id={Id}", id);

            return _mapper.Map<LessonResponse>(lesson);
        }

        public async Task DeleteLessonAsync(int id)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id);

            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);
            if (course.Status == CourseStatus.Published)
            {
                throw new InvalidOperationException($"Cannot delete lesson '{id}' because its parent course is already published.");
            }
            await _lessonRepository.DeleteAsync(id);

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
    }
}
