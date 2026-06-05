using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{
    public class QuizAttempts
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public int UserId { get; set; }
        public double Score { get; set; }
        public bool IsPassed { get; set; }
        public AttemptStatus Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        public Quzzes Quiz { get; set; }
        public Users User { get; set; }
        public ICollection<QuizAnswers> Answers { get; set; } = new List<QuizAnswers>();
    }
}