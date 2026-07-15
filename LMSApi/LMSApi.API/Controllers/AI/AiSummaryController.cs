using Asp.Versioning;
using Hangfire;
using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Services.AI;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace LMSApi.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/lessons/{lessonId:int}/ai")]
    [Authorize]
    public class AiSummaryController : ControllerBase
    {
        private readonly ILessonAiSummaryRepository _aiSummaryRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseSectionRepository _sectionRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<AiSummaryController> _logger;

        public AiSummaryController(
            ILessonAiSummaryRepository aiSummaryRepository,
            ILessonRepository lessonRepository,
            IEnrollmentRepository enrollmentRepository,
            ICourseSectionRepository sectionRepository,
            ICourseRepository courseRepository,
            ILogger<AiSummaryController> logger)
        {
            _aiSummaryRepository = aiSummaryRepository;
            _lessonRepository = lessonRepository;
            _enrollmentRepository = enrollmentRepository;
            _sectionRepository = sectionRepository;
            _courseRepository = courseRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get the AI summary for a lesson.
        /// Returns 202 Accepted while still generating, or 200 OK with the summary.
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<AiSummaryResponse>> GetSummary(int lessonId)
        {
            var userId = User.GetUserId();
            var isAdmin = User.IsAdmin();

            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
                return NotFound($"Lesson {lessonId} not found.");

            if (lesson.Type == LessonType.ExternalLink)
                return Ok(new AiSummaryResponse
                {
                    LessonId = lessonId,
                    Status = "not_supported",
                    Summary = "AI Summary is not available for external link lessons."
                });

            // Check access
            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);

            if (!isAdmin && course.InstructorId != userId)
            {
                var isEnrolled = await _enrollmentRepository.IsAlreadyEnrolledAsync(userId, course.Id);
                if (!isEnrolled)
                    return Forbid();
            }

            var record = await _aiSummaryRepository.GetByLessonIdAsync(lessonId);

            if (record == null)
            {
                // No summary record yet — kick off generation
                BackgroundJob.Enqueue<AiLessonJobService>(j => j.GenerateLessonSummaryAsync(lessonId));
                return Accepted(new AiSummaryResponse
                {
                    LessonId = lessonId,
                    Status = "generating"
                });
            }

            if (record.Status == "generating")
            {
                return Accepted(new AiSummaryResponse
                {
                    LessonId = lessonId,
                    Status = "generating"
                });
            }

            var keyPoints = string.IsNullOrWhiteSpace(record.KeyPointsJson)
                ? []
                : JsonSerializer.Deserialize<List<string>>(record.KeyPointsJson) ?? [];

            return Ok(new AiSummaryResponse
            {
                LessonId = lessonId,
                Summary = record.Summary,
                KeyPoints = keyPoints,
                Notes = record.Notes,
                Status = record.Status,
                GeneratedAt = record.GeneratedAt
            });
        }

        /// <summary>
        /// Manually trigger summary regeneration for a lesson.
        /// Only instructors and admins can call this.
        /// </summary>
        [HttpPost("summary/regenerate")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> RegenerateSummary(int lessonId)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
                return NotFound($"Lesson {lessonId} not found.");

            if (lesson.Type == LessonType.ExternalLink)
                return BadRequest(new { message = "AI Summary is not supported for external link lessons." });

            _logger.LogInformation("Manual summary regeneration triggered for lesson {LessonId}", lessonId);

            BackgroundJob.Enqueue<AiLessonJobService>(j => j.GenerateLessonSummaryAsync(lessonId));
            BackgroundJob.Enqueue<AiLessonJobService>(j => j.IndexLessonForAiAsync(lessonId));

            return Accepted(new { message = "Summary regeneration queued.", lessonId });
        }
    }
}
