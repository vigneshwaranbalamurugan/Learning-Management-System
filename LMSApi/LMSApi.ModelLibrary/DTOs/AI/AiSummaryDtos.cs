namespace LMSApi.ModelLibrary.DTOs
{
    // ─── Responses ───────────────────────────────────────────────────────────

    public class AiSummaryResponse
    {
        public int LessonId { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<string> KeyPoints { get; set; } = [];
        public string Notes { get; set; } = string.Empty;

        /// <summary>"generated" | "generating" | "not_supported" | "error"</summary>
        public string Status { get; set; } = "generating";

        public DateTime? GeneratedAt { get; set; }
    }

    // ─── Internal AI Engine Payloads ─────────────────────────────────────────
    // These are serialised and sent to the Python AI Engine by AiEngineService.

    public class AiEngineIndexRequest
    {
        public int LessonId { get; set; }
        public int LessonType { get; set; }
        public string? ContentUrl { get; set; }
        public string? ContentText { get; set; }
    }

    public class AiEngineIndexResponse
    {
        public int LessonId { get; set; }
        public int ChunksIndexed { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class AiEngineSummaryRequest
    {
        public int LessonId { get; set; }
        public int LessonType { get; set; }
        public string? ContentUrl { get; set; }
        public string? ContentText { get; set; }
    }

    public class AiEngineSummaryResponse
    {
        public int LessonId { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<string> KeyPoints { get; set; } = [];
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class AiEngineChatRequest
    {
        public int LessonId { get; set; }
        public string Question { get; set; } = string.Empty;
        public List<AiChatMessage>? History { get; set; }
        public string? ContentUrl { get; set; }
        public string? ContentText { get; set; }
    }

    public class AiEngineChatResponse
    {
        public string Answer { get; set; } = string.Empty;
        public int SourceLessonId { get; set; }
    }
}
