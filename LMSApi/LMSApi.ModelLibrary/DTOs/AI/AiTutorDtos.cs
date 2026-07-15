using System.ComponentModel.DataAnnotations;

namespace LMSApi.ModelLibrary.DTOs
{
    // ─── Requests ────────────────────────────────────────────────────────────

    public class AiTutorChatRequest
    {
        [Required(ErrorMessage = "Question is required.")]
        [MaxLength(2000, ErrorMessage = "Question must not exceed 2000 characters.")]
        public string Question { get; set; } = string.Empty;

        /// <summary>Prior conversation turns (max 10 will be forwarded to the AI engine).</summary>
        public List<AiChatMessage>? History { get; set; }
    }

    public class AiChatMessage
    {
        /// <summary>"user" or "assistant"</summary>
        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;
    }

    // ─── Responses ───────────────────────────────────────────────────────────

    public class AiTutorChatResponse
    {
        public string Answer { get; set; } = string.Empty;
        public int LessonId { get; set; }
    }
}
