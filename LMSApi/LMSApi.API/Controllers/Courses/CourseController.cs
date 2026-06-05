using Asp.Versioning;
using LMSApi.API.Handlers;
using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMSApi.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly CourseUploadHandler _courseUploadHandler;

        public CoursesController(ICourseService courseService, CourseUploadHandler courseUploadHandler)
        {
            _courseService = courseService;
            _courseUploadHandler = courseUploadHandler;
        }


        /// <summary>Get all published courses.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseResponse>>> GetAll()
        {
            var result = await _courseService.GetAllCoursesAsync();
            return Ok(result);
        }

        /// <summary>Get a course with full details (sections + lessons).</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CourseDetailsResponse>> GetById(int id)
        {
            var result = await _courseService.GetCourseByIdAsync(id);
            return Ok(result);
        }

        /// <summary>Get all courses in a specific category.</summary>
        [HttpGet("category/{categoryId:int}")]
        public async Task<ActionResult<IEnumerable<CourseResponse>>> GetByCategory(int categoryId)
        {
            var result = await _courseService.GetCoursesByCategoryAsync(categoryId);
            return Ok(result);
        }


        [Authorize(Roles = "Instructor,Admin")]
        [HttpGet("my-courses")]
        public async Task<ActionResult<IEnumerable<CourseResponse>>> GetMyCourses()
        {
            var instructorId = User.GetUserId();
            var result = await _courseService.GetCoursesByInstructorAsync(instructorId);
            return Ok(result);
        }


        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<CourseResponse>> Create([FromForm] CreateCourseFormRequest form)
        {
            if (form.Thumbnail != null)
                _courseUploadHandler.ValidateThumbnail(form.Thumbnail);

            if (form.IntroVideo != null)
                _courseUploadHandler.ValidateIntroVideo(form.IntroVideo);

            var instructorId = User.GetUserId();   // always from JWT

            var request = new CreateCourseRequest
            {
                CategoryId        = form.CategoryId,
                Title             = form.Title,
                Description       = form.Description,
                Price             = form.Price,
                IsPremium         = form.IsPremium,
                Requirements      = form.Requirements,
                LearningOutcomes  = form.LearningOutcomes,
                EstimatedDuration = form.EstimatedDuration,
                Level             = form.Level,
                Language          = form.Language,
                // Hybrid Learning
                CourseAccessType              = form.CourseAccessType,
                DefaultAssignmentDeadlineDays = form.DefaultAssignmentDeadlineDays
            };
            Console.WriteLine($"{form.Thumbnail?.FileName} - {form.IntroVideo?.FileName}");
            await using var thumbnailStream = form.Thumbnail?.OpenReadStream();
            await using var videoStream     = form.IntroVideo?.OpenReadStream();
            Console.WriteLine($"Thumbnail Stream: {thumbnailStream != null}, Video Stream: {videoStream != null}");
            var result = await _courseService.CreateCourseAsync(
                instructorId, request,
                thumbnailStream, form.Thumbnail?.FileName,
                videoStream,     form.IntroVideo?.FileName);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<CourseResponse>> Update(int id, [FromForm] UpdateCourseFormRequest form)
        {
            // Ownership check: Instructors may only edit their own courses
            await EnforceOwnershipAsync(id);

            if (form.Thumbnail != null)
                _courseUploadHandler.ValidateThumbnail(form.Thumbnail);

            if (form.IntroVideo != null)
                _courseUploadHandler.ValidateIntroVideo(form.IntroVideo);

            var request = new UpdateCourseRequest
            {
                Title             = form.Title,
                CategoryId        = form.CategoryId,
                Description       = form.Description,
                Price             = form.Price,
                IsPremium         = form.IsPremium,
                Requirements      = form.Requirements,
                LearningOutcomes  = form.LearningOutcomes,
                EstimatedDuration = form.EstimatedDuration,
                Level             = form.Level,
                Language          = form.Language,
                // Hybrid Learning
                CourseAccessType              = form.CourseAccessType,
                DefaultAssignmentDeadlineDays = form.DefaultAssignmentDeadlineDays
            };

            await using var thumbnailStream = form.Thumbnail?.OpenReadStream();
            await using var videoStream     = form.IntroVideo?.OpenReadStream();

            var result = await _courseService.UpdateCourseAsync(
                id, request,
                thumbnailStream, form.Thumbnail?.FileName,
                videoStream,     form.IntroVideo?.FileName);

            return Ok(result);
        }

        /// <summary>Delete a course. Admin only.</summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _courseService.DeleteCourseAsync(id);
            return NoContent();
        }

        /// <summary>Publish a course. Instructor (own only) or Admin.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPatch("{id:int}/publish")]
        public async Task<ActionResult<CourseResponse>> Publish(int id)
        {
            await EnforceOwnershipAsync(id);
            var result = await _courseService.PublishCourseAsync(id);
            return Ok(result);
        }

        /// <summary>Unpublish a course. Instructor (own only) or Admin.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPatch("{id:int}/unpublish")]
        public async Task<ActionResult<CourseResponse>> Unpublish(int id)
        {
            await EnforceOwnershipAsync(id);
            var result = await _courseService.UnpublishCourseAsync(id);
            return Ok(result);
        }


        /// <summary>
        /// Ensures the calling Instructor owns the course.
        /// Admins bypass this check entirely.
        /// </summary>
        private async Task EnforceOwnershipAsync(int courseId)
        {
            if (User.IsAdmin()) return;   // Admins can do anything

            var callerId = User.GetUserId();
            var course = await _courseService.GetCourseByIdAsync(courseId);

            if (course.InstructorId != callerId)
                throw new UnauthorizedAccessException("You do not have permission to modify this course.");
        }
    }
}
