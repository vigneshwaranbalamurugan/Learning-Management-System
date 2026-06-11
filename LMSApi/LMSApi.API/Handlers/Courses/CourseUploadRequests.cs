using System.ComponentModel.DataAnnotations;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.API.Handlers
{
    public class CreateCourseFormRequest:CreateCourseRequest
    {
        [Required(ErrorMessage ="Thumbnail Image is Required")]
        public IFormFile Thumbnail { get; set; }

        /// <summary>Optional course intro video (MP4, MOV, AVI, WEBM).</summary>
        public IFormFile? IntroVideo { get; set; }
    }

    public class UpdateCourseFormRequest:UpdateCourseRequest
    {
        /// <summary>New thumbnail image to replace the existing one (JPG, JPEG, PNG). Leave empty to keep current.</summary>
        public IFormFile? Thumbnail { get; set; }

        /// <summary>New intro video to replace the existing one (MP4, MOV, AVI, WEBM). Leave empty to keep current.</summary>
        public IFormFile? IntroVideo { get; set; }
    }
}
