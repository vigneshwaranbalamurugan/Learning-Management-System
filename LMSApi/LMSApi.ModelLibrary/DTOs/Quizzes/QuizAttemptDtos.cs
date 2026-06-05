using System.ComponentModel.DataAnnotations;

namespace LMSApi.ModelLibrary.DTOs
{
    // ─── Requests ────────────────────────────────────────────────────────────

    public class SubmitQuizRequest
    {
        [Required]
        public int QuizId { get; set; }

        [Required]
        public List<SubmitAnswerItem> Answers { get; set; } = [];
    }

    public class SubmitAnswerItem
    {
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; }
    }

    // ─── Responses ───────────────────────────────────────────────────────────

    public class StartAttemptResponse
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public int UserId { get; set; }
        public DateTime StartedAt { get; set; }
        public TimeSpan TimeLimit { get; set; }
    }

    public class GetRemainingAttemptsResponse
    {
        public int QuizId { get; set; }
        public int RemainingAttempts { get; set; }
        public int MaxAttempts { get; set; }
    }

    public class QuizAttemptResponse
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public int UserId { get; set; }
        public double Score { get; set; }
        public bool IsPassed { get; set; }
        public string Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class QuizAttemptDetailResponse
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public int UserId { get; set; }
        public double Score { get; set; }
        public bool IsPassed { get; set; }
        public string Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<QuizAnswerResponse> Answers { get; set; } = [];
    }

    public class QuizAnswerResponse
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int SelectedOptionId { get; set; }
        public string SelectedOptionText { get; set; }
        public bool IsCorrect { get; set; }
    }
}
