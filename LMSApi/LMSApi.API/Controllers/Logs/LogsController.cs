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
        public async Task<ActionResult> GetActivityLogs(
            [FromQuery] string? userQuery,
            [FromQuery] string? activityType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var logs = await _adminLogService.GetActivityLogsAsync(userQuery, activityType, page, pageSize);
            var totalCount = await _adminLogService.GetActivityLogsCountAsync(userQuery, activityType);
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return Ok(new { logs, totalCount, totalPages });
        }

        [HttpGet("audit")]
        public async Task<ActionResult> GetAuditLogs(
            [FromQuery] string? userQuery,
            [FromQuery] string? tableName,
            [FromQuery] string? action,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var logs = await _adminLogService.GetAuditLogsAsync(userQuery, tableName, action, page, pageSize);
            var totalCount = await _adminLogService.GetAuditLogsCountAsync(userQuery, tableName, action);
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return Ok(new { logs, totalCount, totalPages });
        }

        [HttpGet("audit/{id}")]
        public async Task<ActionResult<AuditLogResponse>> GetAuditLogById(int id)
        {
            try
            {
                var log = await _adminLogService.GetAuditLogByIdAsync(id);
                return Ok(log);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
