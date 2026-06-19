using System.ComponentModel.DataAnnotations;
using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.DTOs
{
    // ─── Requests ────────────────────────────────────────────────────────────

    public class CreateQuizRequest
    {
        [Required(ErrorMessage = "Course Section ID is required.")]
        public int CourseSectionId { get; set; }

        [Required(ErrorMessage = "Quiz title is required.")]
        [MaxLength(300, ErrorMessage = "Title must not exceed 300 characters.")]
        public string Title { get; set; }

        [MaxLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Time Limit is required.")]
        public TimeSpan TimeLimit { get; set; }

        [Range(0, 100, ErrorMessage = "Passing percentage must be between 0 and 100.")]
        public int PassingPercentage { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Max attempts must be at least 1.")]
        public int MaxAttempts { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Order must be zero or greater.")]
        public int Order { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Deadline must be zero or greater.")]
        public int DeadlineInDays { get; set; }

        public DateTime? DeadlineDate { get; set; }
    }

    public class UpdateQuizRequest
    {
        [MaxLength(300, ErrorMessage = "Title must not exceed 300 characters.")]
        public string? Title { get; set; }

        [MaxLength(2000, ErrorMessage = "Description must not exceed 2000 characters.")]
        public string? Description { get; set; }

        public TimeSpan? TimeLimit { get; set; }
        [Range(0, 100, ErrorMessage = "Passing percentage must be between 0 and 100.")]
        public int? PassingPercentage { get; set; }
        public int? MaxAttempts { get; set; }

        [Range(0, int.MaxValue)]
        public int? Order { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Deadline must be zero or greater.")]
        public int? DeadlineInDays { get; set; }

        public DateTime? DeadlineDate { get; set; }

        public PublishStatus? Status { get; set; }
    }

    public class PublishQuizRequest
    {
        [Required]
        public bool Publish { get; set; }
    }

    // ─── Responses ───────────────────────────────────────────────────────────

    public class QuizResponse
    {
        public int Id { get; set; }
        public int CourseSectionId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public TimeSpan TimeLimit { get; set; }
        public int TotalMarks { get; set; }
        public int PassingPercentage { get; set; }
        public int MaxAttempts { get; set; }
        public int Order { get; set; }
        public PublishStatus Status { get; set; }
        public int DeadlineInDays { get; set; }
        public DateTime? DeadlineDate { get; set; }
        public int QuestionCount { get; set; }
    }

    public class QuizDetailResponse
    {
        public int Id { get; set; }
        public int CourseSectionId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public TimeSpan TimeLimit { get; set; }
        public int TotalMarks { get; set; }
        public int PassingPercentage { get; set; }
        public int MaxAttempts { get; set; }
        public int Order { get; set; }
        public PublishStatus Status { get; set; }
        public int DeadlineInDays { get; set; }
        public DateTime? DeadlineDate { get; set; }
        public List<QuizQuestionResponse> Questions { get; set; } = [];
    }

    public class QuizStudentDetailResponse
    {
        public int Id { get; set; }
        public int CourseSectionId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public TimeSpan TimeLimit { get; set; }
        public int TotalMarks { get; set; }
        public int PassingPercentage { get; set; }
        public int MaxAttempts { get; set; }
        public int Order { get; set; }
        public int DeadlineInDays { get; set; }
        public DateTime? DeadlineDate { get; set; }
        public List<QuizStudentQuestionResponse> Questions { get; set; } = [];
    }

    public class BulkUploadResult
    {
        public int TotalImported { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}
