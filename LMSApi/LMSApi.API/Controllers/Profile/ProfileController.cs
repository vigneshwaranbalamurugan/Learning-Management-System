using LMSApi.BALLibrary.Interfaces;
using LMSApi.API.Handlers;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using System.Security.Claims;
using LMSApi.API.Controllers.Profile;

namespace LMSApi.API.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly ProfileImageUploadHandler _profileImageUploadHandler;

        public ProfileController(IProfileService profileService, ProfileImageUploadHandler profileImageUploadHandler)
        {
            _profileService = profileService;
            _profileImageUploadHandler = profileImageUploadHandler;
        }

        [HttpGet("get-profile")]
        public async Task<ActionResult<ProfileResponse>> GetProfile()
        {
            var email = GetCurrentUserEmail();
            var result = await _profileService.GetProfileAsync(email);
            return Ok(result);
        }

        [HttpPut("update-profile")]
        public async Task<ActionResult<ProfileResponse>> UpdateProfile([FromBody] ProfileUpdateRequest request)
        {
            var email = GetCurrentUserEmail();
            var result = await _profileService.UpdateProfileAsync(email, request);
            return Ok(result);
        }

        [HttpPost("update-profile-image")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProfileResponse>> UpdateProfileImage([FromForm] ProfileImageUploadRequest request)
        {
            Console.WriteLine($"Received file: {request.File?.FileName}, size: {request.File?.Length} bytes, content type: {request.File?.ContentType}");
            _profileImageUploadHandler.Validate(request.File);

            var email = GetCurrentUserEmail();
            await using var stream = request.File.OpenReadStream();
            var result = await _profileService.UpdateProfileImageAsync(email, stream, "profile-" + email+".png", request.File.ContentType ?? string.Empty);
            return Ok(result);
        }

        private string GetCurrentUserEmail()
        {
            return User.FindFirstValue(ClaimTypes.Email)
                   ?? User.FindFirstValue("email")
                   ?? throw new UnauthorizedAccessException("Authenticated user email was not found.");
        }
    }

}