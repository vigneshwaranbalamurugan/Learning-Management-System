using System.ComponentModel.DataAnnotations;

namespace LMSApi.ModelLibrary.DTOs
{
    public class LoginRequest
    {
        private string _email;
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { 
            get=> _email;
            set => _email = value?.Trim().ToLowerInvariant();
        }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }

    public class LoginResponse
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiresAt { get; set; }
        public string Message { get; set; }
        public bool RememberMe { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class RefreshTokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime RefreshTokenExpiresAt { get; set; }
        public bool RememberMe { get; set; }
    }

    public class ForgotPasswordRequest
    {
        private string _email;
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email
        {
            get => _email;
            set => _email = value?.Trim().ToLowerInvariant();
        }
    }

    public class ForgotPasswordResponse
    {
        public string Email { get; set; }
        public string Message { get; set; }
    }

    public class ResetPasswordRequest
    {
        private string _email;
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email
        {
            get => _email;
            set => _email = value?.Trim().ToLowerInvariant();
        }

        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; }

        [Required(ErrorMessage = "New Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number.")]
        public string NewPassword { get; set; }
    }

    public class ResetPasswordResponse
    {
        public string Email { get; set; }
        public string Message { get; set; }
    }
}
