using Asp.Versioning;
using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LMSApi.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class QuizAttemptsController : ControllerBase
    {
        private readonly IQuizAttemptService _quizAttemptService;

        public QuizAttemptsController(IQuizAttemptService quizAttemptService)
        {
            _quizAttemptService = quizAttemptService;
        }

        [Authorize]
        [HttpGet("{id:int}/take")]
        public async Task<ActionResult<QuizStudentDetailResponse>> GetQuizForStudent(int id)
        {
            var userId = User.GetUserId();
            var result = await _quizAttemptService.GetQuizForStudentAsync(id, userId);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("{id:int}/start")]
        public async Task<ActionResult<StartAttemptResponse>> StartAttempt(int id)
        {
            var userId = User.GetUserId();
            var result = await _quizAttemptService.StartAttemptAsync(id, userId);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("{id:int}/submit")]
        [EnableRateLimiting("QuizSubmit")]
        public async Task<ActionResult<QuizAttemptResponse>> SubmitQuiz(int id, [FromBody] SubmitQuizRequest request)
        {
            var userId = User.GetUserId();

            var result = await _quizAttemptService.SubmitQuizAsync(id, userId, request);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id:int}/remaining-attempts")]
        public async Task<ActionResult<GetRemainingAttemptsResponse>> GetRemainingAttempts(int id)
        {
            var userId = User.GetUserId();
            var result = await _quizAttemptService.GetRemainingAttemptsAsync(id, userId);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("quiz/{quizId:int}")]
        public async Task<ActionResult<IEnumerable<QuizAttemptResponse>>> GetUserAttempts(int quizId)
        {
            var userId = User.GetUserId();
            var result = await _quizAttemptService.GetUserAttemptsAsync(quizId, userId);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{attemptId:int}")]
        public async Task<ActionResult<QuizAttemptDetailResponse>> GetAttemptDetail(int attemptId)
        {
            var result = await _quizAttemptService.GetAttemptDetailAsync(attemptId);
            return Ok(result);
        }
    }
}
