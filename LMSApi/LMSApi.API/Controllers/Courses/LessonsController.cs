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

        public LessonsController(
            ILessonService lessonService,
            ICourseSectionRepository sectionRepository,
            ICourseService courseService,
            LessonUploadHandler lessonUploadHandler)
        {
            _lessonService = lessonService;
            _sectionRepository = sectionRepository;
            _courseService = courseService;
            _lessonUploadHandler = lessonUploadHandler;
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

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<LessonResponse>> Create([FromForm] CreateLessonFormRequest form)
        {
            // 1. Enforce section ownership
            await EnforceSectionOwnershipAsync(form.CourseSectionId);

            // 2. Validate file upload based on type
            if (form.Type == LessonType.Video)
            {
                if (form.File == null)
                    throw new InvalidOperationException("Video file is required for Video type lessons.");
                _lessonUploadHandler.ValidateLessonVideo(form.File);
            }
            else if (form.Type == LessonType.Pdf)
            {
                if (form.File == null)
                    throw new InvalidOperationException("PDF file is required for PDF type lessons.");
                _lessonUploadHandler.ValidateLessonPdf(form.File);
            }
            else if (form.Type == LessonType.ExternalResource)
            {
                if (string.IsNullOrWhiteSpace(form.ExternalUrl))
                    throw new InvalidOperationException("External URL is required for ExternalResource type lessons.");
            }
            else if (form.Type == LessonType.Text)
            {
                if (string.IsNullOrWhiteSpace(form.Content))
                    throw new InvalidOperationException("Content is required for Text type lessons.");
            }

            var request = new CreateLessonRequest
            {
                CourseSectionId = form.CourseSectionId,
                Title = form.Title,
                Description = form.Description,
                Content = form.Content,
                ExternalUrl = form.ExternalUrl,
                Type = form.Type,
                DurationInMinutes = form.DurationInMinutes,
                Duration = form.Duration,
                SortOrder = form.SortOrder
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

            // Fetch the current lesson to check its current type if not provided in form
            var existingLesson = await _lessonService.GetLessonByIdAsync(id);
            var finalType = form.Type ?? existingLesson.Type;

            // 2. Validate file upload based on type
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
                if (form.Type.Value == LessonType.ExternalResource && string.IsNullOrWhiteSpace(form.ExternalUrl) && string.IsNullOrWhiteSpace(existingLesson.ExternalUrl))
                {
                    throw new InvalidOperationException("External URL is required for ExternalResource type lessons.");
                }
                if (form.Type.Value == LessonType.Text && string.IsNullOrWhiteSpace(form.Content) && string.IsNullOrWhiteSpace(existingLesson.Content))
                {
                    throw new InvalidOperationException("Content is required for Text type lessons.");
                }
            }

            var request = new UpdateLessonRequest
            {
                Title = form.Title,
                Description = form.Description,
                Content = form.Content,
                ExternalUrl = form.ExternalUrl,
                Type = form.Type,
                DurationInMinutes = form.DurationInMinutes,
                Duration = form.Duration,
                SortOrder = form.SortOrder
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
            // Ownership check for reordering lessons:
            // The instructor must own all lessons being reordered.
            foreach (var item in request.LessonOrders)
            {
                await EnforceLessonOwnershipAsync(item.LessonId);
            }

            await _lessonService.ReorderLessonsAsync(request);
            return NoContent();
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
