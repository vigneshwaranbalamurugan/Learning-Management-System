using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Utils;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LMSApi.BALLibrary.Services.AI
{
    /// <summary>
    /// Background job service that orchestrates AI indexing and summary generation.
    /// Invoked by Hangfire after lesson create/update.
    /// </summary>
    public class AiLessonJobService
    {
        private readonly IAiEngineService _aiEngine;
        private readonly ILessonRepository _lessonRepository;
        private readonly ILessonAiSummaryRepository _aiSummaryRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AiLessonJobService> _logger;

        // SAS expiry for AI job processing (60 minutes — enough for transcription)
        private const int AiSasExpiryMinutes = 60;

        public AiLessonJobService(
            IAiEngineService aiEngine,
            ILessonRepository lessonRepository,
            ILessonAiSummaryRepository aiSummaryRepository,
            IConfiguration configuration,
            ILogger<AiLessonJobService> logger)
        {
            _aiEngine = aiEngine;
            _lessonRepository = lessonRepository;
            _aiSummaryRepository = aiSummaryRepository;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Index lesson content into ChromaDB for the AI tutor.
        /// Called for all supported lesson types (Video, PDF, Article).
        /// </summary>
        public async Task IndexLessonForAiAsync(int lessonId)
        {
            _logger.LogInformation("AI Index job started for lesson {LessonId}", lessonId);

            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
            {
                _logger.LogWarning("Lesson {LessonId} not found for AI indexing.", lessonId);
                return;
            }

            if (lesson.Type == LessonType.ExternalLink)
            {
                _logger.LogInformation("Skipping AI indexing for ExternalLink lesson {LessonId}", lessonId);
                return;
            }

            var (contentUrl, contentText) = GetContent(lesson);

            try
            {
                var result = await _aiEngine.IndexLessonAsync(
                    lessonId: lesson.Id,
                    lessonType: (int)lesson.Type,
                    contentUrl: contentUrl,
                    contentText: contentText
                );
                _logger.LogInformation(
                    "AI Index job completed for lesson {LessonId}: {ChunksIndexed} chunks, status={Status}",
                    lessonId, result.ChunksIndexed, result.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI Index job failed for lesson {LessonId}", lessonId);
                // Don't rethrow — indexing failure should not block lesson availability
            }
        }

        /// <summary>
        /// Generate and persist an AI summary for a lesson.
        /// Called for Video, PDF, and Article lessons only.
        /// </summary>
        public async Task GenerateLessonSummaryAsync(int lessonId)
        {
            _logger.LogInformation("AI Summary job started for lesson {LessonId}", lessonId);

            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
            {
                _logger.LogWarning("Lesson {LessonId} not found for summary generation.", lessonId);
                return;
            }

            if (lesson.Type == LessonType.ExternalLink)
            {
                _logger.LogInformation("Skipping AI summary for ExternalLink lesson {LessonId}", lessonId);
                return;
            }

            // Upsert a "generating" placeholder so the frontend can show a skeleton
            var placeholder = await _aiSummaryRepository.GetByLessonIdAsync(lessonId)
                ?? new LessonAiSummary { LessonId = lessonId };
            placeholder.Status = "generating";
            placeholder.GeneratedAt = DateTime.UtcNow;
            await _aiSummaryRepository.UpsertAsync(placeholder);

            var (contentUrl, contentText) = GetContent(lesson);

            try
            {
                var result = await _aiEngine.GenerateSummaryAsync(
                    lessonId: lesson.Id,
                    lessonType: (int)lesson.Type,
                    contentUrl: contentUrl,
                    contentText: contentText
                );

                var summary = await _aiSummaryRepository.GetByLessonIdAsync(lessonId)
                    ?? new LessonAiSummary { LessonId = lessonId };

                summary.Summary = result.Summary;
                summary.KeyPointsJson = JsonSerializer.Serialize(result.KeyPoints);
                summary.Notes = result.Notes;
                summary.Status = result.Status;
                summary.GeneratedAt = DateTime.UtcNow;

                await _aiSummaryRepository.UpsertAsync(summary);

                _logger.LogInformation(
                    "AI Summary job completed for lesson {LessonId}: status={Status}",
                    lessonId, result.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI Summary job failed for lesson {LessonId}", lessonId);

                var errorRecord = await _aiSummaryRepository.GetByLessonIdAsync(lessonId)
                    ?? new LessonAiSummary { LessonId = lessonId };
                errorRecord.Status = "error";
                errorRecord.GeneratedAt = DateTime.UtcNow;
                await _aiSummaryRepository.UpsertAsync(errorRecord);
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// Resolve the content to pass to the AI engine.
        /// For Video/PDF: generate a 60-minute SAS URL directly using AzureBlobUtils.
        /// For Article: pass the raw content text.
        /// </summary>
        private (string? contentUrl, string? contentText) GetContent(Lessons lesson)
        {
            return lesson.Type switch
            {
                LessonType.Video or LessonType.Pdf when !string.IsNullOrWhiteSpace(lesson.ContentUrl) =>
                    (AzureBlobUtils.GenerateSasUrl(_configuration, lesson.ContentUrl, AiSasExpiryMinutes), null),

                LessonType.Article =>
                    (null, lesson.Content),

                _ => (null, null)
            };
        }
    }
}
