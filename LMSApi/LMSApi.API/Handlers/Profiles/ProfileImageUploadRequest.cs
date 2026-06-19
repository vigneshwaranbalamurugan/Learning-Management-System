using System.ComponentModel.DataAnnotations;

namespace LMSApi.API.Handlers
{
    public class ProfileImageUploadRequest
    {
        [Required]
        public IFormFile? File { get; set; }
    }
}