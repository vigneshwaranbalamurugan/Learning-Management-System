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
        
        [HttpPost("courses/{courseId:int}/enroll/free")]
        public async Task<ActionResult<EnrollmentResponse>> EnrollFree(
            int courseId,
            [FromBody] EnrollRequest request)
        {
            var userId = User.GetUserId();
            var result = await _enrollmentService.EnrollInFreeCourseAsync(userId, courseId, request.BatchId);
            return CreatedAtAction(nameof(GetMyEnrollments), null, result);
        }

        [HttpPost("courses/{courseId:int}/enroll/premium")]
        public async Task<ActionResult<object>> EnrollPremium(
            int courseId,
            [FromBody] EnrollRequest request)
        {
            var userId = User.GetUserId();
            var providerName = request.ProviderName ?? "Razorpay"; // Default to Razorpay if not specified
            var orderId = await _enrollmentService.EnrollInPremiumCourseAsync(userId, courseId, request.BatchId, providerName);
            return Ok(new { ProviderOrderId = orderId, ProviderName = providerName });
        }

        [HttpPost("courses/{courseId:int}/enroll/verify")]
        public async Task<ActionResult<EnrollmentResponse>> VerifyPayment(
            int courseId,
            [FromBody] VerifyPaymentRequest request)
        {
            var userId = User.GetUserId();
            var result = await _enrollmentService.VerifyPaymentAndEnrollAsync(
                userId, 
                courseId, 
                request.BatchId, 
                request.ProviderName, 
                request.ProviderOrderId, 
                request.ProviderPaymentId, 
                request.ProviderSignature);
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
