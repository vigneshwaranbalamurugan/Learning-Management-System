using Asp.Versioning;
using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMSApi.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/lessons/{lessonId:int}/ai")]
    [Authorize]
    public class AiTutorController : ControllerBase
    {
        private readonly IAiEngineService _aiEngine;
        private readonly ILessonRepository _lessonRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseSectionRepository _sectionRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<AiTutorController> _logger;

        public AiTutorController(
            IAiEngineService aiEngine,
            ILessonRepository lessonRepository,
            IEnrollmentRepository enrollmentRepository,
            ICourseSectionRepository sectionRepository,
            ICourseRepository courseRepository,
            ILogger<AiTutorController> logger)
        {
            _aiEngine = aiEngine;
            _lessonRepository = lessonRepository;
            _enrollmentRepository = enrollmentRepository;
            _sectionRepository = sectionRepository;
            _courseRepository = courseRepository;
            _logger = logger;
        }

        /// <summary>
        /// Ask the AI tutor a question about this lesson.
        /// Only enrolled students, the lesson's instructor, and admins can use this.
        /// </summary>
        [HttpPost("chat")]
        public async Task<ActionResult<AiTutorChatResponse>> Chat(
            int lessonId,
            [FromBody] AiTutorChatRequest request)
        {
            var userId = User.GetUserId();
            var isAdmin = User.IsAdmin();

            // Validate lesson exists
            var lesson = await _lessonRepository.GetByIdAsync(lessonId);
            if (lesson == null)
                return NotFound($"Lesson {lessonId} not found.");

            // ExternalLink lessons are not supported
            if (lesson.Type == LessonType.ExternalLink)
                return BadRequest(new { message = "AI Tutor is not available for external link lessons." });

            // Check access: admin OR instructor of the course OR enrolled student
            var section = await _sectionRepository.GetByIdAsync(lesson.CourseSectionId);
            var course = await _courseRepository.GetByIdAsync(section.CourseId);

            if (!isAdmin && course.InstructorId != userId)
            {
                var isEnrolled = await _enrollmentRepository.IsAlreadyEnrolledAsync(userId, course.Id);
                if (!isEnrolled)
                    return Forbid();
            }

            _logger.LogInformation("AI Tutor chat: userId={UserId}, lessonId={LessonId}", userId, lessonId);

            var response = await _aiEngine.ChatWithTutorAsync(
                lessonId: lessonId,
                question: request.Question,
                history: request.History,
                contentUrl: null,  // Content is already indexed in ChromaDB
                contentText: null
            );

            return Ok(new AiTutorChatResponse
            {
                Answer = response.Answer,
                LessonId = lessonId
            });
        }
    }
}
