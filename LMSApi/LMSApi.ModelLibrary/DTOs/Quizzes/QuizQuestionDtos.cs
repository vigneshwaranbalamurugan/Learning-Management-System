using System.ComponentModel.DataAnnotations;
using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.DTOs
{
    // ─── Requests ────────────────────────────────────────────────────────────

    public class CreateQuizQuestionRequest
    {
        [Required(ErrorMessage = "Quiz ID is required.")]
        public int QuizId { get; set; }

        [Required(ErrorMessage = "Question text is required.")]
        [MaxLength(2000, ErrorMessage = "Question text must not exceed 2000 characters.")]
        public string QuestionText { get; set; }

        [Required(ErrorMessage = "Question type is required.")]
        public QuestionType QuestionType { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Mark must be at least 1.")]
        public int Mark { get; set; }

        [MaxLength(2000)]
        public string? Explanation { get; set; }

        [Range(0, int.MaxValue)]
        public int SortOrder { get; set; }

        [Required(ErrorMessage = "At least one option is required.")]
        [MinLength(2, ErrorMessage = "A question must have at least 2 options.")]
        public List<CreateQuizOptionRequest> Options { get; set; } = [];
    }

    public class UpdateQuizQuestionRequest
    {
        [MaxLength(2000)]
        public string? QuestionText { get; set; }

        public QuestionType? QuestionType { get; set; }

        [Range(1, int.MaxValue)]
        public int? Mark { get; set; }

        [MaxLength(2000)]
        public string? Explanation { get; set; }

        [Range(0, int.MaxValue)]
        public int? SortOrder { get; set; }

        public List<CreateQuizOptionRequest>? Options { get; set; }
    }

    public class CreateQuizOptionRequest
    {
        [Required(ErrorMessage = "Option text is required.")]
        [MaxLength(1000, ErrorMessage = "Option text must not exceed 1000 characters.")]
        public string OptionText { get; set; }

        public bool IsCorrect { get; set; }
    }

    // ─── Responses ───────────────────────────────────────────────────────────

    public class QuizQuestionResponse
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public int Mark { get; set; }
        public string? Explanation { get; set; }
        public int SortOrder { get; set; }
        public List<QuizOptionResponse> Options { get; set; } = [];
    }

    public class QuizOptionResponse
    {
        public int Id { get; set; }
        public string OptionText { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class QuizStudentQuestionResponse
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public QuestionType QuestionType { get; set; }
        public int Mark { get; set; }
        public int SortOrder { get; set; }
        public List<QuizStudentOptionResponse> Options { get; set; } = [];
    }

    public class QuizStudentOptionResponse
    {
        public int Id { get; set; }
        public string OptionText { get; set; }
    }

    public class BulkReorderQuestionItem
    {
        public int QuestionId { get; set; }
        public int SortOrder { get; set; }
    }

    public class BulkReorderQuestionsRequest
    {
        public List<BulkReorderQuestionItem> Items { get; set; } = [];
    }
}
