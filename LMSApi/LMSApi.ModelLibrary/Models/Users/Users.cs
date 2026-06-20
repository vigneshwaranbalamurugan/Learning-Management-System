using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? PasswordSalt { get; set; }
        public bool IsActive { get; set; }
        public bool IsEmailVerified { get; set; }
        public int RoleId { get; set; }
        public TokenType? CurrentTokenType { get; set; }
        public string? VerificationToken { get; set; }
        public DateTime? VerificationTokenExpiry { get; set; }
        public DateTime LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Navigation property
        public UserRoles Role { get; set; }
        // Navigation for Profiles
        public UserProfiles UserProfile { get; set; }
    }
}