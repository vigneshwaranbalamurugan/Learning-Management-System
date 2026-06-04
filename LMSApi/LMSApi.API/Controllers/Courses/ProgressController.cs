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
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ProgressController : ControllerBase
    {
        private readonly IStudentProgressService _progressService;

        public ProgressController(IStudentProgressService progressService)
        {
            _progressService = progressService;
        }

        /// <summary>Returns the overall course completion progress for the authenticated student.</summary>
        [Authorize]
        [HttpGet("course/{courseId:int}")]
        public async Task<ActionResult<CourseProgressResponse>> GetCourseProgress(int courseId)
        {
            var userId = User.GetUserId();
            var result = await _progressService.GetCourseProgressAsync(userId, courseId);
            return Ok(result);
        }
    }
}
