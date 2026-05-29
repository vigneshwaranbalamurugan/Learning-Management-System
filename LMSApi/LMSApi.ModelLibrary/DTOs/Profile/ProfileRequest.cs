using System.ComponentModel.DataAnnotations;

namespace LMSApi.ModelLibrary.DTOs
{
    public class ProfileUpdateRequest
    {
        [Required(ErrorMessage = "First name is required.")]
        [MinLength(2, ErrorMessage = "First name must be at least 2 characters long.")]
        public string FirstName{get;set;}
        [Required(ErrorMessage = "Last name is required.")]
        [MinLength(1, ErrorMessage = "Last name must be at least 1 characters long.")]
        public string LastName{get;set;}

        [Required(ErrorMessage = "Bio is required.")]
        [MinLength(10, ErrorMessage = "Bio must be at least 10 characters long.")]
        public string Bio{get;set;}
        public DateOnly DateOfBirth{get;set;}
        public string Location{get;set;}
    }

    public class ProfileResponse
    {
        public string? FullName { get; set; } = string.Empty;
        public string? FirstName { get; set; }= string.Empty;
        public string? LastName { get; set; }= string.Empty;
        public string? Bio { get; set; }= string.Empty;
        public DateOnly? DateOfBirth { get; set; }=null;
        public string? Location { get; set; }= string.Empty;
        public string? ProfilePictureUrl { get; set; } = string.Empty;
    }
}