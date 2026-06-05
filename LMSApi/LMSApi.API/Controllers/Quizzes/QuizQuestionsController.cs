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
    public class QuizQuestionsController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly ICourseSectionService _sectionService;
        private readonly ICourseService _courseService;

        public QuizQuestionsController(
            IQuizService quizService,
            ICourseSectionService sectionService,
            ICourseService courseService)
        {
            _quizService = quizService;
            _sectionService = sectionService;
            _courseService = courseService;
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost("quiz/{quizId:int}")]
        public async Task<ActionResult<QuizQuestionResponse>> AddQuestion(int quizId, [FromBody] CreateQuizQuestionRequest request)
        {
            await EnforceQuizOwnershipAsync(quizId);
            request.QuizId = quizId;

            var result = await _quizService.AddQuestionAsync(request);
            return Created($"api/v1/quizquestions/{result.Id}", result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{questionId:int}")]
        public async Task<ActionResult<QuizQuestionResponse>> UpdateQuestion(int questionId, [FromBody] UpdateQuizQuestionRequest request)
        {
            var result = await _quizService.UpdateQuestionAsync(questionId, request);
            return Ok(result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("{questionId:int}")]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            await _quizService.DeleteQuestionAsync(questionId);
            return NoContent();
        }

        private async Task EnforceQuizOwnershipAsync(int quizId)
        {
            if (User.IsAdmin()) return;

            var callerId = User.GetUserId();
            var quiz = await _quizService.GetQuizByIdAsync(quizId);
            var section = await _sectionService.GetSectionByIdAsync(quiz.CourseSectionId);
            var course = await _courseService.GetCourseByIdAsync(section.CourseId);

            if (course.InstructorId != callerId)
                throw new UnauthorizedAccessException("You do not have permission to modify questions in this quiz.");
        }
    }
}
