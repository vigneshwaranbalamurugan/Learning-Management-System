using System.ComponentModel.DataAnnotations;
using LMSApi.ModelLibrary.Enums;
using Microsoft.AspNetCore.Http;

namespace LMSApi.API.Handlers.Courses
{
    public class CreateLessonFormRequest
    {
        [Required(ErrorMessage = "Course section ID is required.")]
        public int CourseSectionId { get; set; }

        [Required(ErrorMessage = "Lesson title is required.")]
        [MaxLength(300, ErrorMessage = "Title must not exceed 300 characters.")]
        public string Title { get; set; }

        [MaxLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
        public string? Description { get; set; }

        public string? Content { get; set; }
        public string? ExternalUrl { get; set; }

        [Required(ErrorMessage = "Lesson type is required.")]
        public LessonType Type { get; set; }

        public TimeSpan? DurationInMinutes { get; set; }
        public TimeSpan Duration { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Sort order must be zero or greater.")]
        public int SortOrder { get; set; }

        /// <summary>Uploaded file (video or PDF) depending on the LessonType.</summary>
        public IFormFile? File { get; set; }
    }

    public class UpdateLessonFormRequest
    {
        [MaxLength(300, ErrorMessage = "Title must not exceed 300 characters.")]
        public string? Title { get; set; }

        [MaxLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
        public string? Description { get; set; }

        public string? Content { get; set; }
        public string? ExternalUrl { get; set; }
        public LessonType? Type { get; set; }
        public TimeSpan? DurationInMinutes { get; set; }
        public TimeSpan? Duration { get; set; }

        [Range(0, int.MaxValue)]
        public int? SortOrder { get; set; }

        /// <summary>Uploaded file (video or PDF) depending on the LessonType.</summary>
        public IFormFile? File { get; set; }
    }
}
