using Asp.Versioning;
using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMSApi.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/batches")]
    public class BatchesController : ControllerBase
    {
        private readonly IBatchService _batchService;
        private readonly ICourseService _courseService;

        public BatchesController(IBatchService batchService, ICourseService courseService)
        {
            _batchService = batchService;
            _courseService = courseService;
        }

        // ─── Public ─────────────────────────────────────────────────────────

        /// <summary>Get all batches for a specific course (public).</summary>
        [HttpGet("/api/v{version:apiVersion}/courses/{courseId:int}/batches")]
        public async Task<ActionResult<IEnumerable<BatchResponse>>> GetBatchesByCourse(int courseId)
        {
            var result = await _batchService.GetBatchesByCourseAsync(courseId);
            return Ok(result);
        }

        // ─── Admin / Instructor ─────────────────────────────────────────────

        /// <summary>Create a new batch for a CohortBased course. Instructor (own only) or Admin.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        public async Task<ActionResult<BatchResponse>> Create([FromBody] CreateBatchRequest request, [FromQuery] int courseId)
        {
            await EnforceCourseOwnershipAsync(courseId);
            var result = await _batchService.CreateBatchAsync(courseId, request);
            return CreatedAtAction(nameof(GetBatchesByCourse), new { courseId = result.CourseId }, result);
        }

        /// <summary>Update an existing batch. Instructor (own only) or Admin.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<BatchResponse>> Update(int id, [FromBody] UpdateBatchRequest request)
        {
            await EnforceBatchOwnershipAsync(id);
            var result = await _batchService.UpdateBatchAsync(id, request);
            return Ok(result);
        }

        /// <summary>Delete a batch. Instructor (own only) or Admin.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await EnforceBatchOwnershipAsync(id);
            await _batchService.DeleteBatchAsync(id);
            return NoContent();
        }

        // ─── Claim helpers ───────────────────────────────────────────────────

        private async Task EnforceCourseOwnershipAsync(int courseId)
        {
            if (User.IsAdmin()) return;

            var callerId = User.GetUserId();
            var course = await _courseService.GetCourseByIdAsync(courseId);

            if (course.InstructorId != callerId)
                throw new UnauthorizedAccessException("You do not have permission to modify batches in this course.");
        }

        private async Task EnforceBatchOwnershipAsync(int batchId)
        {
            if (User.IsAdmin()) return;

            var callerId = User.GetUserId();
            var batch = await _batchService.GetBatchByIdAsync(batchId);
            var course = await _courseService.GetCourseByIdAsync(batch.CourseId);

            if (course.InstructorId != callerId)
                throw new UnauthorizedAccessException("You do not have permission to modify this batch.");
        }
    }
}
