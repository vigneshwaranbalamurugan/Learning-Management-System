using System.ComponentModel.DataAnnotations;

namespace LMSApi.ModelLibrary.DTOs
{
    public class ProfileUpdateRequest
    {
        [Required]
        public string FirstName{get;set;}
        public string LastName{get;set;}
        public string Bio{get;set;}
        public DateOnly DateOfBirth{get;set;}
        public string Location{get;set;}
    }

    public class ProfileResponse
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string Location { get; set; }
        public string ProfilePictureUrl { get; set; }
    }
}