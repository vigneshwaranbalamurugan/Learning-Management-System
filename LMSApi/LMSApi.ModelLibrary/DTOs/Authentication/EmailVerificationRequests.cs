using System.ComponentModel.DataAnnotations;

namespace LMSApi.ModelLibrary.DTOs
{
    public class VerifyEmailRequest
    {

        public string _email;
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { 
            get=> _email;
            set => _email = value?.Trim().ToLowerInvariant();
        }

        [Required]
        public string Token { get; set; }
    }

    public class ResendVerificationRequest
    {
        public string _email;
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { 
            get=> _email;
            set => _email = value?.Trim().ToLowerInvariant();
        }
    }

    public class VerifyEmailResponse
    {
        public bool IsVerified { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
    }
    public class ResendVerificationResponse
    {
        public bool IsSent { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
    }
}
