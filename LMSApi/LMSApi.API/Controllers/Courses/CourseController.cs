using Asp.Versioning;
using LMSApi.API.Handlers.Courses;
using LMSApi.API.Handlers;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMSApi.API.Controllers
{
    [Authorize]
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

        // ─── Queries (all authenticated users) ──────────────────────────────

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

        /// <summary>
        /// Get all courses belonging to the calling instructor.
        /// Admins may pass any instructorId; Instructors always see their own courses only.
        /// </summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpGet("my-courses")]
        public async Task<ActionResult<IEnumerable<CourseResponse>>> GetMyCourses()
        {
            var instructorId = GetCurrentUserId();
            var result = await _courseService.GetCoursesByInstructorAsync(instructorId);
            return Ok(result);
        }

        // ─── Mutations (Instructor / Admin) ─────────────────────────────────

        /// <summary>
        /// Create a new course. Accepts multipart/form-data.
        /// InstructorId is taken automatically from your JWT token — do NOT send it in the form.
        /// Include Thumbnail (optional image) and IntroVideo (optional video) alongside course fields.
        /// </summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<CourseResponse>> Create([FromForm] CreateCourseFormRequest form)
        {
            if (form.Thumbnail != null)
                _courseUploadHandler.ValidateThumbnail(form.Thumbnail);

            if (form.IntroVideo != null)
                _courseUploadHandler.ValidateIntroVideo(form.IntroVideo);

            var instructorId = GetCurrentUserId();   // always from JWT

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
                Language          = form.Language
            };

            await using var thumbnailStream = form.Thumbnail?.OpenReadStream();
            await using var videoStream     = form.IntroVideo?.OpenReadStream();

            var result = await _courseService.CreateCourseAsync(
                instructorId, request,
                thumbnailStream, form.Thumbnail?.FileName,
                videoStream,     form.IntroVideo?.FileName);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Update a course. Accepts multipart/form-data.
        /// Instructors can only update their own courses. Admins can update any course.
        /// Include Thumbnail / IntroVideo only when you want to replace the file.
        /// </summary>
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
                Language          = form.Language
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

        // ─── Claim helpers ───────────────────────────────────────────────────

        /// <summary>Extracts the authenticated user's DB id from the JWT NameIdentifier claim.</summary>
        private int GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? throw new UnauthorizedAccessException("User ID claim not found in token.");

            return int.TryParse(value, out var id)
                ? id
                : throw new UnauthorizedAccessException("User ID claim is not a valid integer.");
        }

        private bool IsAdmin() =>
            User.IsInRole("Admin");

        /// <summary>
        /// Ensures the calling Instructor owns the course.
        /// Admins bypass this check entirely.
        /// </summary>
        private async Task EnforceOwnershipAsync(int courseId)
        {
            if (IsAdmin()) return;   // Admins can do anything

            var callerId = GetCurrentUserId();
            var course = await _courseService.GetCourseByIdAsync(courseId);

            if (course.InstructorId != callerId)
                throw new UnauthorizedAccessException("You do not have permission to modify this course.");
        }
    }
}
