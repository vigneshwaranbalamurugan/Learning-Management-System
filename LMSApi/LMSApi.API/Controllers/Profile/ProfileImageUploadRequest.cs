using System.ComponentModel.DataAnnotations;

namespace LMSApi.API.Controllers
{
    public class ProfileImageUploadRequest
    {
        [Required]
        public IFormFile File { get; set; }
    }
}