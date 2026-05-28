using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.DTOs
{
    public class RegisterRequest
    {
        private string _email;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email{ 
            get=> _email;
            set => _email = value?.Trim().ToLowerInvariant();
        }

        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number.")]
        public string Password { get; set; }

        [Required]
        [EnumDataType(typeof(RegistrationRole), ErrorMessage = "Invalid registration role. Only Learner and Instructor roles are allowed.")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RegistrationRole Role { get; set; }

    }

    public class RegisterResponse
    {
        public string Email { get; set; }
        public string Message { get; set; }
    }

}