using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;

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
		[EnableRateLimiting("Register")]
		public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
		{
			var result = await _authService.RegisterAsync(request);
			return Ok(result);
		}

		[HttpGet("verify")]
		[EnableRateLimiting("OtpVerify")]
		public async Task<IActionResult> VerifyFromQuery([FromQuery] string email, [FromQuery] string token)
		{
	
			var req = new VerifyEmailRequest { Email = email, Token = token };
			var result = await _authService.VerifyEmailAsync(req);
			return Ok(result);
		}

		[HttpPost("resend")]
		[EnableRateLimiting("OtpSend")]
		public async Task<ActionResult<ResendVerificationResponse>> Resend([FromBody] ResendVerificationRequest request)
		{
			var result = await _authService.ReRequestEmailVerificationAsync(request);
			return Ok(result);
		}

		[HttpPost("login")]
		[EnableRateLimiting("Login")]
		public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
		{
			var result = await _authService.AuthenticateAsync(request);
			return Ok(result);
		}

		[HttpPost("forgot-password")]
		[EnableRateLimiting("ForgotPassword")]
		public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
		{
			var result = await _authService.ForgotPasswordAsync(request);
			return Ok(result);
		}

		[HttpPost("reset-password")]
		public async Task<ActionResult<ResetPasswordResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
		{
			var result = await _authService.ResetPasswordAsync(request);
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
