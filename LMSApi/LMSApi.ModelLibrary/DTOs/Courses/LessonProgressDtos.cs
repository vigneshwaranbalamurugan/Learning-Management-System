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
    }
}
