using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{
    public class Quzzes
    {
        public int Id { get; set; }
        public int CourseSectionId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public TimeSpan TimeLimit { get; set; }

        public int PassingPercentage { get; set; }
        public int MaxAttempts { get; set; }
        public int Order { get; set; }
        public PublishStatus Status { get; set; } = PublishStatus.Draft;
        public int DeadlineInDays { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public CourseSection CourseSection { get; set; }
        public ICollection<QuizQuestions> Questions { get; set; } = new List<QuizQuestions>();
        public ICollection<QuizAttempts> Attempts { get; set; } = new List<QuizAttempts>();
    }
}