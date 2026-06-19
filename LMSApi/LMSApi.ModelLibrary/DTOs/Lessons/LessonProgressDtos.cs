using System;

namespace LMSApi.ModelLibrary.DTOs
{
    public class CompleteLessonRequest
    {
        public decimal? WatchPercentage { get; set; }
    }

    public class LessonProgressResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int LessonId { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime LastViewedAt { get; set; }
        public decimal WatchPercentage { get; set; }
    }

    public class CourseProgressResponse
    {
        public int CourseId { get; set; }
        public decimal ProgressPercentage { get; set; }
        public int CompletedLessonsCount { get; set; }
        public int TotalLessonsCount { get; set; }

        public IEnumerable<SectionProgressResponse> Sections { get; set; } = [];
    }

    public class SectionProgressResponse
    {
        public int SectionId { get; set; }
        public string Title { get; set; }
        public decimal ProgressPercentage { get; set; }
        public IEnumerable<LessonProgressResponse> Lessons { get; set; } = [];
        public IEnumerable<QuizProgressResponse> Quizzes { get; set; } = [];
        public IEnumerable<AssignmentProgressResponse> Assignments { get; set; } = [];
    }

    public class QuizProgressResponse
    {
        public int QuizId { get; set; }
        public bool IsPassed { get; set; }
        public int AttemptsMade { get; set; }
    }

    public class AssignmentProgressResponse
    {
        public int AssignmentId { get; set; }
        public bool IsPassed { get; set; }
        public string Status { get; set; }
    }
}
