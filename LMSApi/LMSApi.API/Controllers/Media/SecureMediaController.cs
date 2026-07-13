using Asp.Versioning;
using LMSApi.BALLibrary.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMSApi.API.Controllers.Media
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/media")]
    [AllowAnonymous]
    public class SecureMediaController : ControllerBase
    {
        private readonly ISecureMediaService _secureMediaService;

        public SecureMediaController(ISecureMediaService secureMediaService)
        {
            _secureMediaService = secureMediaService;
        }

        [HttpGet("secure-url")]
        public async Task<IActionResult> GetSecureUrl([FromQuery] string blobPath, [FromQuery] int courseId)
        {
            if (string.IsNullOrWhiteSpace(blobPath))
                return BadRequest(new { Message = "blobPath query parameter is required." });

            if (courseId <= 0)
                return BadRequest(new { Message = "courseId query parameter is required." });

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? userId = null;
            if (int.TryParse(userIdClaim, out var parsed)) userId = parsed;

            try
            {
                var secureUrl = await _secureMediaService.GetSecureUrlAsync(blobPath, userId, courseId);
                return Ok(new { Url = secureUrl });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(); // 403 Forbidden
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }
    }
}
