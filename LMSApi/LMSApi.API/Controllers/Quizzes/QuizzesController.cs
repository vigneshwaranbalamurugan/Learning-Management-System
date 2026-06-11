using Asp.Versioning;
using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMSApi.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class QuizzesController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly ICourseSectionService _sectionService;
        private readonly ICourseService _courseService;

        public QuizzesController(
            IQuizService quizService,
            ICourseSectionService sectionService,
            ICourseService courseService)
        {
            _quizService = quizService;
            _sectionService = sectionService;
            _courseService = courseService;
        }

        [Authorize]
        [HttpGet("section/{sectionId:int}")]
        public async Task<ActionResult<IEnumerable<QuizResponse>>> GetBySection(int sectionId)
        {
            var userId = User.GetUserId();
            var isAdmin = User.IsAdmin();
            var result = await _quizService.GetQuizzesBySectionAsync(sectionId, userId, isAdmin);
            return Ok(result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<QuizDetailResponse>> GetById(int id)
        {
            var userId = User.GetUserId();
            var isAdmin = User.IsAdmin();
            var result = await _quizService.GetQuizByIdAsync(id, userId, isAdmin);
            return Ok(result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        public async Task<ActionResult<QuizResponse>> Create([FromBody] CreateQuizRequest request)
        {
            await EnforceSectionOwnershipAsync(request.CourseSectionId);

            var result = await _quizService.CreateQuizAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<QuizResponse>> Update(int id, [FromBody] UpdateQuizRequest request)
        {
            await EnforceQuizOwnershipAsync(id);

            var result = await _quizService.UpdateQuizAsync(id, request);
            return Ok(result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await EnforceQuizOwnershipAsync(id);
            await _quizService.DeleteQuizAsync(id);
            return NoContent();
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id:int}/publish")]
        public async Task<ActionResult<QuizResponse>> Publish(int id, [FromBody] PublishQuizRequest request)
        {
            await EnforceQuizOwnershipAsync(id);
            var result = await _quizService.PublishQuizAsync(id, request);
            return Ok(result);
        }

        private async Task EnforceSectionOwnershipAsync(int sectionId)
        {
            if (User.IsAdmin()) return;

            var callerId = User.GetUserId();
            var section = await _sectionService.GetSectionByIdAsync(sectionId, callerId, User.IsAdmin());
            var course = await _courseService.GetCourseByIdAsync(section.CourseId, callerId, User.IsAdmin());

            if (course.InstructorId != callerId)
                throw new UnauthorizedAccessException("You do not have permission to manage quizzes in this section.");
        }

        private async Task EnforceQuizOwnershipAsync(int quizId)
        {
            if (User.IsAdmin()) return;

            var callerId = User.GetUserId();
            var quiz = await _quizService.GetQuizByIdAsync(quizId, callerId, User.IsAdmin());
            var section = await _sectionService.GetSectionByIdAsync(quiz.CourseSectionId, callerId, User.IsAdmin());
            var course = await _courseService.GetCourseByIdAsync(section.CourseId, callerId, User.IsAdmin());

            if (course.InstructorId != callerId)
                throw new UnauthorizedAccessException("You do not have permission to modify this quiz.");
        }
    }
}
