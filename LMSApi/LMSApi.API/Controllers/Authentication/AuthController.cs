using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace LMSApi.API.Controllers
{
	[ApiController]
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;

		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpPost("register")]
		public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
		{
			var result = await _authService.RegisterAsync(request);
			return Ok(result);
		}

		[HttpGet("verify")]
		public async Task<IActionResult> VerifyFromQuery([FromQuery] string email, [FromQuery] string token)
		{
	
			var req = new VerifyEmailRequest { Email = email, Token = token };
			var result = await _authService.VerifyEmailAsync(req);
			return Ok(result);
		}

		[HttpPost("resend")]
		public async Task<ActionResult<ResendVerificationResponse>> Resend([FromBody] ResendVerificationRequest request)
		{
			var result = await _authService.ReRequestEmailVerificationAsync(request);
			return Ok(result);
		}

		[HttpPost("login")]
		public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
		{
			var result = await _authService.AuthenticateAsync(request);
			return Ok(result);
		}

		[Authorize]
		[HttpGet("protected")]
		public async Task<ActionResult> Protected()
		{
			return Ok("This is a protected endpoint.");
		}
	}
}
