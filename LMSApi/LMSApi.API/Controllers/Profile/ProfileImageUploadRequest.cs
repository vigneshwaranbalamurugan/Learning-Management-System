using System.ComponentModel.DataAnnotations;

namespace LMSApi.API.Controllers.Profile
{
    public class ProfileImageUploadRequest
    {
        [Required]
        public IFormFile File { get; set; }
    }
}