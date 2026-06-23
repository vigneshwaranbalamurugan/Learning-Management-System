using Asp.Versioning;
using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LMSApi.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class QuizQuestionsController : ControllerBase
    {
        private readonly IQuizQuestionService _quizQuestionService;
        private readonly IOwnershipService _ownershipService;

        public QuizQuestionsController(
            IQuizQuestionService quizQuestionService,
            IOwnershipService ownershipService)
        {
            _quizQuestionService = quizQuestionService;
            _ownershipService = ownershipService;
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost("quiz/{quizId:int}")]
        public async Task<ActionResult<QuizQuestionResponse>> AddQuestion(int quizId, [FromBody] CreateQuizQuestionRequest request)
        {
            await _ownershipService.EnforceQuizOwnershipAsync(quizId, User.GetUserId(), User.IsAdmin(), "You do not have permission to modify questions in this quiz.");
            request.QuizId = quizId;

            var result = await _quizQuestionService.AddQuestionAsync(request);
            return Created($"api/v1/quizquestions/{result.Id}", result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost("quiz/{quizId:int}/bulk")]
        public async Task<ActionResult<BulkUploadResult>> BulkUploadQuestions(int quizId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty or not provided.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".xlsx")
                return BadRequest("Invalid file format. Please upload an .xlsx file.");

            await _ownershipService.EnforceQuizOwnershipAsync(quizId, User.GetUserId(), User.IsAdmin(), "You do not have permission to modify questions in this quiz.");

            using var stream = file.OpenReadStream();
            var result = await _quizQuestionService.BulkUploadQuestionsAsync(quizId, stream);

            if (result.TotalImported == 0 && result.Errors.Any())
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{questionId:int}")]
        public async Task<ActionResult<QuizQuestionResponse>> UpdateQuestion(int questionId, [FromBody] UpdateQuizQuestionRequest request)
        {
            var result = await _quizQuestionService.UpdateQuestionAsync(questionId, request);
            return Ok(result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost("quiz/{quizId:int}/reorder")]
        public async Task<IActionResult> ReorderQuestions(int quizId, [FromBody] BulkReorderQuestionsRequest request)
        {
            await _ownershipService.EnforceQuizOwnershipAsync(quizId, User.GetUserId(), User.IsAdmin(), "You do not have permission to reorder questions in this quiz.");
            await _quizQuestionService.ReorderQuestionsAsync(quizId, request);
            return NoContent();
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("{questionId:int}")]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            await _quizQuestionService.DeleteQuestionAsync(questionId);
            return NoContent();
        }

    }
}
