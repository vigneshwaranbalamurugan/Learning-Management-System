using Asp.Versioning;
using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using LMSApi.API.Handlers.Courses;
using LMSApi.API.Handlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMSApi.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class LessonsController : ControllerBase
    {
        private readonly ILessonService _lessonService;
        private readonly ICourseSectionRepository _sectionRepository;
        private readonly ICourseService _courseService;
        private readonly LessonUploadHandler _lessonUploadHandler;
        private readonly IStudentProgressService _progressService;

        public LessonsController(
            ILessonService lessonService,
            ICourseSectionRepository sectionRepository,
            ICourseService courseService,
            LessonUploadHandler lessonUploadHandler,
            IStudentProgressService progressService)
        {
            _lessonService = lessonService;
            _sectionRepository = sectionRepository;
            _courseService = courseService;
            _lessonUploadHandler = lessonUploadHandler;
            _progressService = progressService;
        }

        [Authorize]
        [HttpGet("section/{sectionId:int}")]
        public async Task<ActionResult<IEnumerable<LessonResponse>>> GetBySection(int sectionId)
        {
            var result = await _lessonService.GetLessonsBySectionAsync(sectionId);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<LessonResponse>> GetById(int id)
        {
            var result = await _lessonService.GetLessonByIdAsync(id);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id:int}/detail")]
        public async Task<ActionResult<LessonDetailResponse>> GetDetail(int id)
        {
            var result = await _lessonService.GetLessonDetailAsync(id);
            return Ok(result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<LessonResponse>> Create([FromForm] CreateLessonFormRequest form)
        {
            // 1. Enforce section ownership
            await EnforceSectionOwnershipAsync(form.CourseSectionId);

            // 2. Validate required fields per lesson type
            switch (form.Type)
            {
                case LessonType.Video:
                    if (form.File == null)
                        throw new InvalidOperationException("Video file is required for Video type lessons.");
                    _lessonUploadHandler.ValidateLessonVideo(form.File);
                    break;

                case LessonType.Pdf:
                    if (form.File == null)
                        throw new InvalidOperationException("PDF file is required for Pdf type lessons.");
                    _lessonUploadHandler.ValidateLessonPdf(form.File);
                    break;

                case LessonType.ExternalLink:
                    if (string.IsNullOrWhiteSpace(form.ContentUrl))
                        throw new InvalidOperationException("ContentUrl is required for ExternalLink type lessons.");
                    break;

                case LessonType.Article:
                    if (string.IsNullOrWhiteSpace(form.Content))
                        throw new InvalidOperationException("Content is required for Article type lessons.");
                    break;
            }

            var request = new CreateLessonRequest
            {
                CourseSectionId = form.CourseSectionId,
                Title = form.Title,
                Description = form.Description,
                Content = form.Content,
                ContentUrl = form.ContentUrl,
                Type = form.Type,
                DurationInMinutes = form.DurationInMinutes,
                SortOrder = form.SortOrder,
                IsPreview = form.IsPreview ?? false,
                IsPublished = form.IsPublished ?? false
            };

            await using var fileStream = form.File?.OpenReadStream();

            var result = await _lessonService.CreateLessonAsync(request, fileStream, form.File?.FileName);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<LessonResponse>> Update(int id, [FromForm] UpdateLessonFormRequest form)
        {
            // 1. Enforce lesson ownership
            await EnforceLessonOwnershipAsync(id);

            // Fetch the current lesson to check its type if not changed by form
            var existingLesson = await _lessonService.GetLessonByIdAsync(id);
            var finalType = form.Type ?? existingLesson.Type;

            // 2. Validate file upload based on resolved type
            if (form.File != null)
            {
                if (finalType == LessonType.Video)
                {
                    _lessonUploadHandler.ValidateLessonVideo(form.File);
                }
                else if (finalType == LessonType.Pdf)
                {
                    _lessonUploadHandler.ValidateLessonPdf(form.File);
                }
                else
                {
                    throw new InvalidOperationException($"Files cannot be uploaded to lessons of type {finalType}.");
                }
            }

            // 3. Additional validations if changing type
            if (form.Type.HasValue)
            {
                if (form.Type.Value == LessonType.ExternalLink
                    && string.IsNullOrWhiteSpace(form.ContentUrl)
                    && string.IsNullOrWhiteSpace(existingLesson.ContentUrl))
                {
                    throw new InvalidOperationException("ContentUrl is required for ExternalLink type lessons.");
                }

                if (form.Type.Value == LessonType.Article
                    && string.IsNullOrWhiteSpace(form.Content)
                    && string.IsNullOrWhiteSpace(existingLesson.Content))
                {
                    throw new InvalidOperationException("Content is required for Article type lessons.");
                }
            }

            var request = new UpdateLessonRequest
            {
                Title = form.Title,
                Description = form.Description,
                Content = form.Content,
                ContentUrl = form.ContentUrl,
                Type = form.Type,
                DurationInMinutes = form.DurationInMinutes,
                SortOrder = form.SortOrder,
                IsPreview = form.IsPreview,
                IsPublished = form.IsPublished
            };

            await using var fileStream = form.File?.OpenReadStream();

            var result = await _lessonService.UpdateLessonAsync(id, request, fileStream, form.File?.FileName);
            return Ok(result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await EnforceLessonOwnershipAsync(id);
            await _lessonService.DeleteLessonAsync(id);
            return NoContent();
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("reorder")]
        public async Task<IActionResult> Reorder([FromBody] ReorderLessonsRequest request)
        {
            // Ownership check: the instructor must own all lessons being reordered
            foreach (var item in request.LessonOrders)
            {
                await EnforceLessonOwnershipAsync(item.LessonId);
            }

            await _lessonService.ReorderLessonsAsync(request);
            return NoContent();
        }

        /// <summary>
        /// Mark a lesson complete.
        /// For Video lessons, pass WatchPercentage; auto-completes at ≥ 90%.
        /// For Pdf / Article / ExternalLink, marking this endpoint always completes the lesson.
        /// </summary>
        [Authorize]
        [HttpPost("{lessonId:int}/complete")]
        public async Task<ActionResult<LessonProgressResponse>> Complete(int lessonId, [FromBody] CompleteLessonRequest? request)
        {
            var userId = User.GetUserId();
            var result = await _progressService.MarkLessonCompleteAsync(userId, lessonId, request?.WatchPercentage);
            return Ok(result);
        }

        // ─── Claim helpers ───────────────────────────────────────────────────

        private async Task EnforceSectionOwnershipAsync(int sectionId)
        {
            if (User.IsAdmin()) return;

            var callerId = User.GetUserId();
            var section = await _sectionRepository.GetByIdAsync(sectionId);
            var course = await _courseService.GetCourseByIdAsync(section.CourseId);

            if (course.InstructorId != callerId)
                throw new UnauthorizedAccessException("You do not have permission to modify lessons in this section.");
        }

        private async Task EnforceLessonOwnershipAsync(int lessonId)
        {
            if (User.IsAdmin()) return;

            var callerId = User.GetUserId();
            var lesson = await _lessonService.GetLessonByIdAsync(lessonId);
            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId);
            var course = await _courseService.GetCourseByIdAsync(section.CourseId);

            if (course.InstructorId != callerId)
                throw new UnauthorizedAccessException("You do not have permission to modify this lesson.");
        }
    }
}
