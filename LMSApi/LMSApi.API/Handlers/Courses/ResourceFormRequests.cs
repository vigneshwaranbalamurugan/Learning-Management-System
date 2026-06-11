using System.ComponentModel.DataAnnotations;
using LMSApi.ModelLibrary.Enums;
using Microsoft.AspNetCore.Http;

namespace LMSApi.API.Handlers
{
    public class CreateResourceFormRequest
    {
        [Required(ErrorMessage = "Lesson ID is required.")]
        public int LessonId { get; set; }

        [Required(ErrorMessage = "Resource type is required.")]
        public ResourceType ResourceType { get; set; }

        [Required(ErrorMessage = "Resource title is required.")]
        [MaxLength(300, ErrorMessage = "Resource title must not exceed 300 characters.")]
        public string ResourceTitle { get; set; }

        public string? ResourceUrl { get; set; }

        [MaxLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
        public string? Description { get; set; }

        public PublishStatus Status { get; set; }

        public IFormFile? File { get; set; }
    }

    public class UpdateResourceFormRequest
    {
        public ResourceType? ResourceType { get; set; }

        [MaxLength(300, ErrorMessage = "Resource title must not exceed 300 characters.")]
        public string? ResourceTitle { get; set; }

        public string? ResourceUrl { get; set; }

        [MaxLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
        public string? Description { get; set; }

        public PublishStatus? Status { get; set; }

        public IFormFile? File { get; set; }
    }
}
