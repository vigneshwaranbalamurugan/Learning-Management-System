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
        private readonly IOwnershipService _ownershipService;

        public QuizzesController(
            IQuizService quizService,
            IOwnershipService ownershipService)
        {
            _quizService = quizService;
            _ownershipService = ownershipService;
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
            await _ownershipService.EnforceSectionOwnershipAsync(request.CourseSectionId, User.GetUserId(), User.IsAdmin(), "You do not have permission to manage quizzes in this section.");

            var result = await _quizService.CreateQuizAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<QuizResponse>> Update(int id, [FromBody] UpdateQuizRequest request)
        {
            await _ownershipService.EnforceQuizOwnershipAsync(id, User.GetUserId(), User.IsAdmin());

            var result = await _quizService.UpdateQuizAsync(id, request);
            return Ok(result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _ownershipService.EnforceQuizOwnershipAsync(id, User.GetUserId(), User.IsAdmin());
            await _quizService.DeleteQuizAsync(id);
            return NoContent();
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("{id:int}/publish")]
        public async Task<ActionResult<QuizResponse>> Publish(int id, [FromBody] PublishQuizRequest request)
        {
            await _ownershipService.EnforceQuizOwnershipAsync(id, User.GetUserId(), User.IsAdmin());
            var result = await _quizService.PublishQuizAsync(id, request);
            return Ok(result);
        }

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPut("reorder")]
        public async Task<IActionResult> Reorder([FromBody] ReorderQuizzesRequest request)
        {
            foreach (var item in request.QuizOrders)
            {
                await _ownershipService.EnforceQuizOwnershipAsync(item.QuizId, User.GetUserId(), User.IsAdmin());
            }

            await _quizService.ReorderQuizzesAsync(request);
            return NoContent();
        }

    }
}
