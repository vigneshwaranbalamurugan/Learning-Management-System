using Asp.Versioning;
using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LMSApi.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/batches")]
    public class BatchesController : ControllerBase
    {
        private readonly IBatchService _batchService;
        private readonly IOwnershipService _ownershipService;

        public BatchesController(IBatchService batchService, IOwnershipService ownershipService)
        {
            _batchService = batchService;
            _ownershipService = ownershipService;
        }


        /// <summary>Get all batches for a specific course (public).</summary>
        [HttpGet("/api/v{version:apiVersion}/courses/{courseId:int}/batches")]
        [EnableRateLimiting("PublicCourseListing")]
        public async Task<ActionResult<IEnumerable<BatchResponse>>> GetBatchesByCourse(int courseId)
        {
            var result = await _batchService.GetBatchesByCourseAsync(courseId);
            return Ok(result);
        }


        /// <summary>Create a new batch for a CohortBased course. Instructor (own only) or Admin.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        public async Task<ActionResult<BatchResponse>> Create([FromBody] CreateBatchRequest request, [FromQuery] int courseId)
        {
            await _ownershipService.EnforceCourseOwnershipAsync(courseId, User.GetUserId(), User.IsAdmin(), "You do not have permission to modify batches in this course.");
            var result = await _batchService.CreateBatchAsync(courseId, request);
            return CreatedAtAction(nameof(GetBatchesByCourse), new { courseId = result.CourseId }, result);
        }

        /// <summary>Update an existing batch. Instructor (own only) or Admin.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<BatchResponse>> Update(int id, [FromBody] UpdateBatchRequest request)
        {
            await _ownershipService.EnforceBatchOwnershipAsync(id, User.GetUserId(), User.IsAdmin());
            var result = await _batchService.UpdateBatchAsync(id, request);
            return Ok(result);
        }

        /// <summary>Delete a batch. Instructor (own only) or Admin.</summary>
        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _ownershipService.EnforceBatchOwnershipAsync(id, User.GetUserId(), User.IsAdmin());
            await _batchService.DeleteBatchAsync(id);
            return NoContent();
        }

    }
}
