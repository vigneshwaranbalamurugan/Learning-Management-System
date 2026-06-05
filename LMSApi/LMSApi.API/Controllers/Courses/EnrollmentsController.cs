using Asp.Versioning;
using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMSApi.API.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentsController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }
        
        /// <summary>Enroll the authenticated student in a course. Accepts an optional BatchId for CohortBased courses.</summary>
        [HttpPost("courses/{courseId:int}/enroll")]
        public async Task<ActionResult<EnrollmentResponse>> Enroll(
            int courseId,
            [FromBody] EnrollRequest request)
        {
            var userId = User.GetUserId();
            var result = await _enrollmentService.EnrollAsync(userId, courseId, request.BatchId);
            return CreatedAtAction(nameof(GetMyEnrollments), null, result);
        }


        /// <summary>Get all enrollments for the authenticated student.</summary>
        [HttpGet("enrollments/my")]
        public async Task<ActionResult<IEnumerable<EnrollmentResponse>>> GetMyEnrollments()
        {
            var userId = User.GetUserId();
            var result = await _enrollmentService.GetMyEnrollmentsAsync(userId);
            return Ok(result);
        }
    }
}
