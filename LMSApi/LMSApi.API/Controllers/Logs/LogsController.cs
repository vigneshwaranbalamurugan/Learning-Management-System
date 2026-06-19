using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs.Logs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;

namespace LMSApi.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [EnableRateLimiting("AdminApis")]
    public class LogsController : ControllerBase
    {
        private readonly IAdminLogService _adminLogService;

        public LogsController(IAdminLogService adminLogService)
        {
            _adminLogService = adminLogService;
        }

        [HttpGet("activity")]
        public async Task<ActionResult<IEnumerable<ActivityLogResponse>>> GetActivityLogs(
            [FromQuery] int? userId,
            [FromQuery] string? activityType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var logs = await _adminLogService.GetActivityLogsAsync(userId, activityType, page, pageSize);
            return Ok(logs);
        }

        [HttpGet("audit")]
        public async Task<ActionResult<IEnumerable<AuditLogResponse>>> GetAuditLogs(
            [FromQuery] int? userId,
            [FromQuery] string? tableName,
            [FromQuery] string? action,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var logs = await _adminLogService.GetAuditLogsAsync(userId, tableName, action, page, pageSize);
            return Ok(logs);
        }
    }
}
