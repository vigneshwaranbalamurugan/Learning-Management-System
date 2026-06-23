using Asp.Versioning;
using LMSApi.API.Handlers;
using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;

namespace LMSApi.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly CourseUploadHandler _courseUploadHandler;
        private readonly IOwnershipService _ownershipService;

        public CoursesController(
            ICourseService courseService,
            CourseUploadHandler courseUploadHandler,
            IOwnershipService ownershipService)
        {
            _courseService = courseService;
            _courseUploadHandler = courseUploadHandler;
            _ownershipService = ownershipService;
        }


        /// <summary>Get all published courses with pagination and filters.</summary>
        [HttpGet]
        [EnableRateLimiting("PublicCourseListing")]
        public async Task<ActionResult<PagedCourseResponse>> GetAll([FromQuery] CourseSearchQuery query)
        {
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                try
                {
                    userId = User.GetUserId();
                }
                catch (UnauthorizedAccessException)
                {
                }
            }

            var result = await _courseService.GetPublishedCoursesPagedAsync(query, userId);
            return Ok(result);
        }

        /// <summary>Get categories, languages, and active instructors metadata for catalog filtering.</summary>
        [HttpGet("filters-metadata")]
        [EnableRateLimiting("PublicCourseListing")]
        public async Task<ActionResult<FiltersMetadataResponse>> GetFiltersMetadata()
        {
            var result = await _courseService.GetFiltersMetadataAsync();
            return Ok(result);
        }

        /// <summary>Get a course with full details (sections + lessons).</summary>
        [HttpGet("{id:int}")]
        [EnableRateLimiting("PublicCourseListing")]
        public async Task<ActionResult<CourseDetailsResponse>> GetById(int id)
        {
            int? userId = null;
            bool isAdmin = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                try
                {
                    userId = User.GetUserId();
                    isAdmin = User.IsAdmin();
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
            var result = await _courseService.GetCourseByIdAsync(id, userId, isAdmin);
            return Ok(result);
        }

        /// <summary>Get a course with full details (sections + lessons) by slug.</summary>
        [HttpGet("slug/{slug}")]
        [EnableRateLimiting("PublicCourseListing")]
        public async Task<ActionResult<CourseDetailsResponse>> GetBySlug(string slug)
        {
            int? userId = null;
            bool isAdmin = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                try
                {
                    userId = User.GetUserId();
                    isAdmin = User.IsAdmin();
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
            var result = await _courseService.GetCourseBySlugAsync(slug, userId, isAdmin);
            return Ok(result);
        }

        /// <summary>Get course details by slug for instructor workspace (enforces ownership).</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpGet("instructor/slug/{slug}")]
        public async Task<ActionResult<CourseDetailsResponse>> GetInstructorCourseBySlug(string slug)
        {
            var userId = User.GetUserId();
            var isAdmin = User.IsAdmin();
            var result = await _courseService.GetCourseBySlugAsync(slug, userId, isAdmin);
            
            // Enforce ownership: Instructor must be the owner of the course
            await _ownershipService.EnforceCourseOwnershipAsync(result.Id, userId, isAdmin);
            
            return Ok(result);
        }

        /// <summary>Get all courses in a specific category.</summary>
        [HttpGet("category/{categoryId:int}")]
        [EnableRateLimiting("PublicCourseListing")]
        public async Task<ActionResult<IEnumerable<CourseResponse>>> GetByCategory(int categoryId)
        {
            var result = await _courseService.GetCoursesByCategoryAsync(categoryId);
            return Ok(result);
        }


        [Authorize(Roles = "Instructor,Admin")]
        [HttpGet("my-courses")]
        public async Task<ActionResult<PagedCourseResponse>> GetMyCourses([FromQuery] CourseSearchQuery query)
        {
            var instructorId = User.GetUserId();
            var result = await _courseService.GetCoursesByInstructorPagedAsync(instructorId, query);
            return Ok(result);
        }


        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        [EnableRateLimiting("FileUpload")]
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
                LanguageId        = form.LanguageId,
                // Hybrid Learning
                CourseAccessType              = form.CourseAccessType,
                DefaultDeadlineDays = form.DefaultDeadlineDays
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
        [EnableRateLimiting("FileUpload")]
        public async Task<ActionResult<CourseResponse>> Update(int id, [FromForm] UpdateCourseFormRequest form)
        {
            // Ownership check: Instructors may only edit their own courses
            await _ownershipService.EnforceCourseOwnershipAsync(id, User.GetUserId(), User.IsAdmin());

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
                LanguageId        = form.LanguageId,
                // Hybrid Learning
                CourseAccessType              = form.CourseAccessType,
                DefaultDeadlineDays = form.DefaultDeadlineDays
            };

            await using var thumbnailStream = form.Thumbnail?.OpenReadStream();
            await using var videoStream     = form.IntroVideo?.OpenReadStream();

            var result = await _courseService.UpdateCourseAsync(
                id, request,
                thumbnailStream, form.Thumbnail?.FileName,
                videoStream,     form.IntroVideo?.FileName);

            return Ok(result);
        }

        /// <summary>Delete a course. Instructor (own only) or Admin.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _ownershipService.EnforceCourseOwnershipAsync(id, User.GetUserId(), User.IsAdmin());
            await _courseService.DeleteCourseAsync(id);
            return NoContent();
        }

        /// <summary>Publish or unpublish a course. Instructor (own only) or Admin.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPatch("{id:int}/publish")]
        public async Task<ActionResult<CourseResponse>> Publish(int id, [FromBody] PublishCourseRequest request)
        {
            await _ownershipService.EnforceCourseOwnershipAsync(id, User.GetUserId(), User.IsAdmin());
            var result = await _courseService.PublishCourseAsync(id, request);
            return Ok(result);
        }

        /// <summary>Archive or unarchive a course. Instructor (own only) or Admin.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPatch("{id:int}/archive")]
        public async Task<ActionResult<CourseResponse>> Archive(int id, [FromBody] ArchiveCourseRequest request)
        {
            await _ownershipService.EnforceCourseOwnershipAsync(id, User.GetUserId(), User.IsAdmin());
            var result = await _courseService.ArchiveCourseAsync(id, request);
            return Ok(result);
        }

        /// <summary>Get all courses pending approval. Admin only.</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("pending")]
        [EnableRateLimiting("AdminApis")]
        public async Task<ActionResult<IEnumerable<CourseResponse>>> GetPending()
        {
            var result = await _courseService.GetPendingCoursesAsync();
            return Ok(result);
        }

        /// <summary>Review (Approve or Reject) a course. Admin only.</summary>
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:int}/review")]
        [EnableRateLimiting("AdminApis")]
        public async Task<ActionResult<CourseResponse>> Review(int id, [FromBody] ReviewCourseRequest request)
        {
            var result = await _courseService.ReviewCourseAsync(id, request);
            return Ok(result);
        }


    }
}
