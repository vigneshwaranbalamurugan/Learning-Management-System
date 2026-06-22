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

			// SameSite=Lax + Secure=false works over HTTP (LAN/dev).
			// Change to SameSite=None + Secure=true when deploying over HTTPS.
			var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
			{
				HttpOnly = true,
				Secure = false,
				SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
				Expires = result.ExpiresAt
			};
			Response.Cookies.Append("access_token", result.Token, cookieOptions);

			var refreshCookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
			{
				HttpOnly = true,
				Secure = false,
				SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
				Expires = DateTime.UtcNow.AddDays(7)
			};
			Response.Cookies.Append("refresh_token", result.RefreshToken, refreshCookieOptions);

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

		[HttpPost("refresh-token")]
		public async Task<ActionResult<RefreshTokenResponse>> Refresh([FromBody] RefreshTokenRequest? request)
		{
			var accessToken = request?.AccessToken ?? Request.Cookies["access_token"];
			var refreshToken = request?.RefreshToken ?? Request.Cookies["refresh_token"];

			if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
			{
				return Unauthorized("Access token or refresh token is missing.");
			}

			var req = new RefreshTokenRequest { AccessToken = accessToken, RefreshToken = refreshToken };
			var result = await _authService.RefreshTokenAsync(req);

			var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
			{
				HttpOnly = true,
				//For Network
				// Secure = false,
				// SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
				//For Localhost
				Secure=true,
				SameSite=Microsoft.AspNetCore.Http.SameSiteMode.None,
				Expires = result.ExpiresAt
			};
			Response.Cookies.Append("access_token", result.AccessToken, cookieOptions);

			var refreshCookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
			{
				HttpOnly = true,
				// For Network
				// Secure = false,
				// SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
				//For Localhost
				Secure=true,
				SameSite=Microsoft.AspNetCore.Http.SameSiteMode.None,
				Expires = DateTime.UtcNow.AddDays(7)
			};
			Response.Cookies.Append("refresh_token", result.RefreshToken, refreshCookieOptions);

			return Ok(result);
		}

		[Authorize]
		[HttpPost("revoke")]
		public async Task<IActionResult> Revoke()
		{
			var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
			if (string.IsNullOrEmpty(email)) return BadRequest("Invalid user claims");
			await _authService.RevokeTokenAsync(email);

			var deleteCookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
			{
				HttpOnly = true,
				//For Network
				// Secure = false,
				// SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax

				//For Localhost
				Secure=true,
				SameSite=Microsoft.AspNetCore.Http.SameSiteMode.None,
			};
			Response.Cookies.Delete("access_token", deleteCookieOptions);
			Response.Cookies.Delete("refresh_token", deleteCookieOptions);

			return Ok(new { Message = "Token revoked successfully" });
		}
	}
}
