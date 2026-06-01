using Asp.Versioning;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMSApi.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        /// <summary>Get all published courses. Accessible by all authenticated users.</summary>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseResponse>>> GetAll()
        {
            var result = await _courseService.GetAllCoursesAsync();
            return Ok(result);
        }

        /// <summary>Get a course with full details (sections + lessons).</summary>
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CourseDetailsResponse>> GetById(int id)
        {
            var result = await _courseService.GetCourseByIdAsync(id);
            return Ok(result);
        }

        /// <summary>Get all courses by a specific instructor.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpGet("instructor/{instructorId:int}")]
        public async Task<ActionResult<IEnumerable<CourseResponse>>> GetByInstructor(int instructorId)
        {
            var result = await _courseService.GetCoursesByInstructorAsync(instructorId);
            return Ok(result);
        }

        /// <summary>Get all courses in a specific category.</summary>
        [Authorize]
        [HttpGet("category/{categoryId:int}")]
        public async Task<ActionResult<IEnumerable<CourseResponse>>> GetByCategory(int categoryId)
        {
            var result = await _courseService.GetCoursesByCategoryAsync(categoryId);
            return Ok(result);
        }

        /// <summary>Create a new course. Instructor and Admin only.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        public async Task<ActionResult<CourseResponse>> Create([FromBody] CreateCourseRequest request)
        {
            var result = await _courseService.CreateCourseAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>Update a course. Instructor and Admin only.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<CourseResponse>> Update(int id, [FromBody] UpdateCourseRequest request)
        {
            var result = await _courseService.UpdateCourseAsync(id, request);
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

        /// <summary>Publish a course. Instructor and Admin only.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPatch("{id:int}/publish")]
        public async Task<ActionResult<CourseResponse>> Publish(int id)
        {
            var result = await _courseService.PublishCourseAsync(id);
            return Ok(result);
        }

        /// <summary>Unpublish (revert to Draft) a course. Instructor and Admin only.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPatch("{id:int}/unpublish")]
        public async Task<ActionResult<CourseResponse>> Unpublish(int id)
        {
            var result = await _courseService.UnpublishCourseAsync(id);
            return Ok(result);
        }
    }
}
