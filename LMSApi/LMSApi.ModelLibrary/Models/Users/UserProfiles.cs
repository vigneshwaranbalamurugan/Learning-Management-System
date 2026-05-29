namespace LMSApi.ModelLibrary.Models
{
    public class UserProfiles
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public string ProfilePictureUrl { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Location { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation property
        public Users User { get; set; }
    }
}