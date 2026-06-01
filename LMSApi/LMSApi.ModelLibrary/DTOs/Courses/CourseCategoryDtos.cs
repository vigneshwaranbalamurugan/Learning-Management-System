using System.ComponentModel.DataAnnotations;

namespace LMSApi.ModelLibrary.DTOs
{
    // ─── Requests ────────────────────────────────────────────────────────────

    public class CreateCategoryRequest
    {
        [Required(ErrorMessage = "Category name is required.")]
        [MaxLength(150, ErrorMessage = "Category name must not exceed 150 characters.")]
        public string Name { get; set; }

        [MaxLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
        public string? Description { get; set; }
    }

    public class UpdateCategoryRequest
    {
        [MaxLength(150, ErrorMessage = "Category name must not exceed 150 characters.")]
        public string? Name { get; set; }

        [MaxLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
        public string? Description { get; set; }
    }

    // ─── Responses ───────────────────────────────────────────────────────────

    public class CategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
