using System.ComponentModel.DataAnnotations;
using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.DTOs
{
    // ─── Requests ────────────────────────────────────────────────────────────

    public class PublishResourceRequest
    {
        public bool Publish { get; set; }
    }

    public class CreateResourceRequest
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
    }

    public class UpdateResourceRequest
    {
        public ResourceType? ResourceType { get; set; }

        [MaxLength(300, ErrorMessage = "Resource title must not exceed 300 characters.")]
        public string? ResourceTitle { get; set; }

        public string? ResourceUrl { get; set; }

        [MaxLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
        public string? Description { get; set; }

        public PublishStatus? Status { get; set; }
    }

    public class ReorderResourcesRequest
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one resource must be provided.")]
        public List<ResourceOrderItem> Resources { get; set; } = new();
    }

    public class ResourceOrderItem
    {
        [Required]
        public int ResourceId { get; set; }

        [Required]
        public int SortOrder { get; set; }
    }

    // ─── Responses ───────────────────────────────────────────────────────────

    public class ResourceResponse
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public ResourceType ResourceType { get; set; }
        public string ResourceTitle { get; set; }
        public string ResourceUrl { get; set; }
        public string? Description { get; set; }
        public PublishStatus Status { get; set; }
        public int SortOrder { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
