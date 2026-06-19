using LMSApi.BALLibrary.Interfaces;
using LMSApi.API.Handlers;
using LMSApi.API.Extensions;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

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
            var email = User.GetEmail();
            var result = await _profileService.GetProfileAsync(email);
            return Ok(result);
        }

        [HttpPut("update-profile")]
        public async Task<ActionResult<ProfileResponse>> UpdateProfile([FromBody] ProfileUpdateRequest request)
        {
            var email = User.GetEmail();
            var result = await _profileService.UpdateProfileAsync(email, request);
            return Ok(result);
        }

        [HttpPost("update-profile-image")]
        [Consumes("multipart/form-data")]
        [EnableRateLimiting("FileUpload")]
        public async Task<ActionResult<ProfileResponse>> UpdateProfileImage([FromForm] ProfileImageUploadRequest request)
        {
            Console.WriteLine($"Received file: {request.File?.FileName}, size: {request.File?.Length} bytes, content type: {request.File?.ContentType}");
            if (request.File == null) return BadRequest("File is required.");
            _profileImageUploadHandler.Validate(request.File);

            var email = User.GetEmail();
            await using var stream = request.File.OpenReadStream();
            var result = await _profileService.UpdateProfileImageAsync(email, stream, "profile-" + email+".png", request.File.ContentType ?? string.Empty);
            return Ok(result);
        }

    }
}