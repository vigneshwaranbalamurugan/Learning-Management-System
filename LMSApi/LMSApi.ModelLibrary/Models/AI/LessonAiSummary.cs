namespace LMSApi.ModelLibrary.Models
{
    /// <summary>
    /// Stores AI-generated summary, key points, and notes for a lesson.
    /// One row per lesson; regenerated automatically when lesson content changes.
    /// </summary>
    public class LessonAiSummary
    {
        public int Id { get; set; }

        public int LessonId { get; set; }

        /// <summary>3–5 sentence paragraph summarising the lesson.</summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>JSON-serialised array of key point strings.</summary>
        public string KeyPointsJson { get; set; } = "[]";

        /// <summary>Study notes with important terms and tips.</summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>Status returned by the AI engine: "generated", "error", "generating".</summary>
        public string Status { get; set; } = "generating";

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Lessons Lesson { get; set; } = null!;
    }
}
