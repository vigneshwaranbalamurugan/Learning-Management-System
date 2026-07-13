using Asp.Versioning;
using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMSApi.API.Filters;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Http.Timeouts;

namespace LMSApi.API.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly IStudentProgressService _progressService;

        public EnrollmentsController(IEnrollmentService enrollmentService, IStudentProgressService progressService)
        {
            _enrollmentService = enrollmentService;
            _progressService = progressService;
        }
        
        [HttpPost("courses/{courseId:int}/enroll/free")]
        [Idempotency(Required = true, TtlMinutes = 30)]
        [EnableRateLimiting("EnrollCourse")]
        [RequestTimeout("Normal")]
        public async Task<ActionResult<EnrollmentResponse>> EnrollFree(
            int courseId,
            [FromBody] EnrollRequest request)
        {
            var userId = User.GetUserId();
            var result = await _enrollmentService.EnrollInFreeCourseAsync(userId, courseId, request.BatchId);
            return CreatedAtAction(nameof(GetMyEnrollments), null, result);
        }

        [HttpPost("courses/{courseId:int}/enroll/premium")]
        [Idempotency(Required = true, TtlMinutes = 30)]
        [EnableRateLimiting("PaymentInitialization")]
        [RequestTimeout("Normal")]
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
        [Idempotency(Required = true, TtlMinutes = 30)]
        [EnableRateLimiting("EnrollCourse")]
        [RequestTimeout("Normal")]
        public async Task<ActionResult<EnrollmentResponse>> VerifyPayment(
            int courseId,
            [FromBody] VerifyPaymentRequest request)
        {
            var userId = User.GetUserId();
            var result = await _enrollmentService.VerifyPaymentAndEnrollAsync(
                userId, 
                courseId, 
                request);
            return CreatedAtAction(nameof(GetMyEnrollments), null, result);
        }


        [HttpPost("courses/{courseId:int}/update-version")]
        [RequestTimeout("Normal")]
        public async Task<ActionResult<EnrollmentResponse>> UpdateToLatestVersion(int courseId)
        {
            var userId = User.GetUserId();
            var result = await _enrollmentService.UpdateToLatestVersionAsync(userId, courseId);
            await _progressService.RecalculateCourseProgressAsync(userId, courseId);
            // Refresh the result since progress might have changed
            result.ProgressPercentage = (await _progressService.GetCourseProgressAsync(userId, courseId)).ProgressPercentage;
            return Ok(result);
        }


        /// <summary>Get all enrollments for the authenticated student.</summary>
        [HttpGet("enrollments/my")]
        [RequestTimeout("Quick")]
        public async Task<ActionResult<PagedEnrollmentResponse>> GetMyEnrollments(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] string? accessType = null)
        {
            var userId = User.GetUserId();
            var result = await _enrollmentService.GetMyEnrollmentsPagedAsync(userId, page, pageSize, search, status, accessType);
            return Ok(result);
        }
    }
}
