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
    public class QuizAttemptsController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizAttemptsController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        [Authorize]
        [HttpGet("{id:int}/take")]
        public async Task<ActionResult<QuizStudentDetailResponse>> GetQuizForStudent(int id)
        {
            var result = await _quizService.GetQuizForStudentAsync(id);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("{id:int}/start")]
        public async Task<ActionResult<StartAttemptResponse>> StartAttempt(int id)
        {
            var userId = User.GetUserId();
            var result = await _quizService.StartAttemptAsync(id, userId);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("{id:int}/submit")]
        public async Task<ActionResult<QuizAttemptResponse>> SubmitQuiz(int id, [FromBody] SubmitQuizRequest request)
        {
            var userId = User.GetUserId();
            request.QuizId = id;

            var result = await _quizService.SubmitQuizAsync(userId, request);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id:int}/remaining-attempts")]
        public async Task<ActionResult<GetRemainingAttemptsResponse>> GetRemainingAttempts(int id)
        {
            var userId = User.GetUserId();
            var result = await _quizService.GetRemainingAttemptsAsync(id, userId);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("quiz/{quizId:int}")]
        public async Task<ActionResult<IEnumerable<QuizAttemptResponse>>> GetUserAttempts(int quizId)
        {
            var userId = User.GetUserId();
            var result = await _quizService.GetUserAttemptsAsync(quizId, userId);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{attemptId:int}")]
        public async Task<ActionResult<QuizAttemptDetailResponse>> GetAttemptDetail(int attemptId)
        {
            var result = await _quizService.GetAttemptDetailAsync(attemptId);
            return Ok(result);
        }
    }
}
