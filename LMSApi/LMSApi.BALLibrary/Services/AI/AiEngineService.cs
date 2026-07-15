using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace LMSApi.BALLibrary.Services.AI
{
    /// <summary>
    /// HTTP client that communicates with the internal Python FastAPI AI Engine.
    /// All request/response JSON keys use snake_case to match the Python Pydantic models.
    /// </summary>
    public class AiEngineService : IAiEngineService
    {
        private readonly HttpClient _http;
        private readonly ILogger<AiEngineService> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };

        public AiEngineService(
            IHttpClientFactory httpClientFactory,
            ILogger<AiEngineService> logger)
        {
            _http = httpClientFactory.CreateClient("AiEngine");
            _logger = logger;
        }

        // ── Tutor Chat ────────────────────────────────────────────────────────

        public async Task<AiEngineChatResponse> ChatWithTutorAsync(
            int lessonId,
            string question,
            List<AiChatMessage>? history,
            string? contentUrl,
            string? contentText,
            CancellationToken ct = default)
        {
            var payload = new AiEngineChatRequest
            {
                LessonId = lessonId,
                Question = question,
                History = history,
                ContentUrl = contentUrl,
                ContentText = contentText
            };

            _logger.LogInformation("Sending tutor chat request for lesson {LessonId}", lessonId);

            var response = await _http.PostAsJsonAsync(
                "/internal/tutor/chat", payload, _jsonOptions, ct);

            await EnsureSuccessAsync(response, "tutor/chat", lessonId);

            var result = await response.Content.ReadFromJsonAsync<AiEngineChatResponse>(_jsonOptions, ct);
            return result ?? new AiEngineChatResponse { Answer = "No response from AI engine." };
        }

        // ── Index Lesson ──────────────────────────────────────────────────────

        public async Task<AiEngineIndexResponse> IndexLessonAsync(
            int lessonId,
            int lessonType,
            string? contentUrl,
            string? contentText,
            CancellationToken ct = default)
        {
            var payload = new AiEngineIndexRequest
            {
                LessonId = lessonId,
                LessonType = lessonType,
                ContentUrl = contentUrl,
                ContentText = contentText
            };

            _logger.LogInformation("Sending index request for lesson {LessonId} (type={LessonType})", lessonId, lessonType);

            var response = await _http.PostAsJsonAsync(
                "/internal/index/lesson", payload, _jsonOptions, ct);

            await EnsureSuccessAsync(response, "index/lesson", lessonId);

            var result = await response.Content.ReadFromJsonAsync<AiEngineIndexResponse>(_jsonOptions, ct);
            return result ?? new AiEngineIndexResponse { LessonId = lessonId, Status = "error" };
        }

        // ── Generate Summary ──────────────────────────────────────────────────

        public async Task<AiEngineSummaryResponse> GenerateSummaryAsync(
            int lessonId,
            int lessonType,
            string? contentUrl,
            string? contentText,
            CancellationToken ct = default)
        {
            var payload = new AiEngineSummaryRequest
            {
                LessonId = lessonId,
                LessonType = lessonType,
                ContentUrl = contentUrl,
                ContentText = contentText
            };

            _logger.LogInformation("Sending summary request for lesson {LessonId} (type={LessonType})", lessonId, lessonType);

            var response = await _http.PostAsJsonAsync(
                "/internal/summary/generate", payload, _jsonOptions, ct);

            await EnsureSuccessAsync(response, "summary/generate", lessonId);

            var result = await response.Content.ReadFromJsonAsync<AiEngineSummaryResponse>(_jsonOptions, ct);
            return result ?? new AiEngineSummaryResponse { LessonId = lessonId, Status = "error" };
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private async Task EnsureSuccessAsync(HttpResponseMessage response, string endpoint, int lessonId)
        {
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "AI Engine returned {StatusCode} for {Endpoint}, lesson {LessonId}: {Body}",
                    (int)response.StatusCode, endpoint, lessonId, body[..Math.Min(body.Length, 500)]);
                response.EnsureSuccessStatusCode(); // throws HttpRequestException
            }
        }
    }
}
