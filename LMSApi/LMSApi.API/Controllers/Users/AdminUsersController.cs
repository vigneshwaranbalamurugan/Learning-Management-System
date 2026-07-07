using LMSApi.BALLibrary.Interfaces.Users;
using LMSApi.ModelLibrary.DTOs.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Asp.Versioning;

namespace LMSApi.API.Controllers.Users
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;
        private readonly ILogger<AdminUsersController> _logger;

        public AdminUsersController(IAdminUserService adminUserService, ILogger<AdminUsersController> logger)
        {
            _adminUserService = adminUserService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserSearchQuery query)
        {
            _logger.LogInformation("Admin requested user list.");
            var response = await _adminUserService.GetUsersPagedAsync(query);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            _logger.LogInformation("Admin is creating a new user with email {Email} and role {Role}", request.Email, request.Role);
            
            try
            {
                var response = await _adminUserService.CreateUserAsync(request);
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
