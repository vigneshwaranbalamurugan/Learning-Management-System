using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Timeouts;

namespace LMSApi.API.Controllers
{
	[ApiController]
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	[RequestTimeout("Quick")]
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

			var cookieOptions = new CookieOptions
			{
				HttpOnly = true,
				// For Network
				// Secure = false,
				// SameSite = SameSiteMode.Lax,
				//For Localhost
				Secure=true,
				SameSite=SameSiteMode.None
			};
			Response.Cookies.Append("access_token", result.Token, cookieOptions);

			var refreshCookieOptions = new CookieOptions
			{
				HttpOnly = true,
				Secure=true,
				SameSite=SameSiteMode.None
				// Secure = false,
				// SameSite = SameSiteMode.Lax
			};
			if (result.RememberMe)
			{
				refreshCookieOptions.Expires = result.RefreshTokenExpiresAt;
			}
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
			var refreshToken = request?.RefreshToken ?? Request.Cookies["refresh_token"];

			if (string.IsNullOrEmpty(refreshToken))
			{
				return Unauthorized("Refresh token is missing.");
			}

			var req = new RefreshTokenRequest { RefreshToken = refreshToken };
			var result = await _authService.RefreshTokenAsync(req);
			
			var cookieOptions = new CookieOptions
			{
				HttpOnly = true,
				//For Network
				// Secure = false,
				// SameSite = SameSiteMode.Lax,
				//For Localhost
				Secure=true,
				SameSite=SameSiteMode.None
			};
			Response.Cookies.Append("access_token", result.AccessToken, cookieOptions);

			var refreshCookieOptions = new CookieOptions
			{
				HttpOnly = true,
				//For Network
				// Secure = false,
				// SameSite = SameSiteMode.Lax,
				//For Localhost
				Secure=true,
				SameSite=SameSiteMode.None
			};
			if (result.RememberMe)
			{
				refreshCookieOptions.Expires = result.RefreshTokenExpiresAt;
			}
			Response.Cookies.Append("refresh_token", result.RefreshToken, refreshCookieOptions);

			return Ok(result);
		}

		[Authorize]
		[HttpPost("revoke")]
		public async Task<IActionResult> Revoke()
		{
			var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
			if (string.IsNullOrEmpty(email)) return BadRequest("Invalid user claims");
			
			string? jti = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;
			var expClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Exp)?.Value;
			TimeSpan? ttl = null;
			
			if (long.TryParse(expClaim, out var expUnix))
			{
			    var expDate = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
			    var remaining = expDate - DateTime.UtcNow;
			    if (remaining > TimeSpan.Zero)
			    {
			        ttl = remaining;
			    }
			}

			await _authService.RevokeTokenAsync(email, jti, ttl);

			var deleteCookieOptions = new CookieOptions
			{
				HttpOnly = true,
				//For Network
				// Secure = false,
				// SameSite = SameSiteMode.Lax

				//For Localhost
				Secure=true,
				SameSite=SameSiteMode.None,
			};
			Response.Cookies.Delete("access_token", deleteCookieOptions);
			Response.Cookies.Delete("refresh_token", deleteCookieOptions);

			return Ok(new { Message = "Token revoked successfully" });
		}
	}
}
