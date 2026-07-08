using Asp.Versioning;
using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Http.Timeouts;

namespace LMSApi.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [RequestTimeout("Heavy")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>Get overall platform analytics. Admin only.</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        [EnableRateLimiting("AdminApis")]
        public async Task<ActionResult<AdminAnalyticsResponse>> GetAdminAnalytics()
        {
            var result = await _analyticsService.GetAdminAnalyticsAsync();
            return Ok(result);
        }

        /// <summary>Get paginated recent activities. Admin only.</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/activities")]
        [EnableRateLimiting("AdminApis")]
        public async Task<ActionResult<System.Collections.Generic.List<RecentActivityDto>>> GetAdminActivities([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            var result = await _analyticsService.GetAdminRecentActivitiesAsync(page, pageSize);
            return Ok(result);
        }

        /// <summary>Get instructor dashboard analytics. Instructor only.</summary>
        [Authorize(Roles = "Instructor")]
        [HttpGet("instructor")]
        public async Task<ActionResult<InstructorAnalyticsResponse>> GetInstructorAnalytics()
        {
            var instructorId = User.GetUserId();
            var result = await _analyticsService.GetInstructorAnalyticsAsync(instructorId);
            return Ok(result);
        }

        /// <summary>Get learner progress dashboard analytics. Learner only.</summary>
        [Authorize(Roles = "Learner")]
        [HttpGet("learner")]
        public async Task<ActionResult<LearnerAnalyticsResponse>> GetLearnerAnalytics()
        {
            var learnerId = User.GetUserId();
            var result = await _analyticsService.GetLearnerAnalyticsAsync(learnerId);
            return Ok(result);
        }
    }
}
