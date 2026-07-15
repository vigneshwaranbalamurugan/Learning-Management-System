using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    /// <summary>
    /// HTTP client gateway that communicates with the internal Python AI Engine.
    /// The .NET backend is the ONLY caller — the frontend never reaches the AI engine directly.
    /// </summary>
    public interface IAiEngineService
    {
        /// <summary>
        /// Send a student's question to the AI tutor and receive an answer
        /// generated via RAG over the indexed lesson content.
        /// </summary>
        Task<AiEngineChatResponse> ChatWithTutorAsync(
            int lessonId,
            string question,
            List<AiChatMessage>? history,
            string? contentUrl,
            string? contentText,
            CancellationToken ct = default);

        /// <summary>
        /// Ask the AI engine to index a lesson's content into the vector store (ChromaDB).
        /// Called by a Hangfire background job after lesson create/update.
        /// </summary>
        Task<AiEngineIndexResponse> IndexLessonAsync(
            int lessonId,
            int lessonType,
            string? contentUrl,
            string? contentText,
            CancellationToken ct = default);

        /// <summary>
        /// Ask the AI engine to generate a summary, key points, and notes for a lesson.
        /// Called by a Hangfire background job after lesson create/update.
        /// </summary>
        Task<AiEngineSummaryResponse> GenerateSummaryAsync(
            int lessonId,
            int lessonType,
            string? contentUrl,
            string? contentText,
            CancellationToken ct = default);
    }
}
