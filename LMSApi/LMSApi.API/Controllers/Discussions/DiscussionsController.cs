using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using LMSApi.API.Middlewares;
using LMSApi.API.Extensions;
namespace LMSApi.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class DiscussionsController : ControllerBase
    {
        private readonly IDiscussionService _discussionService;
        public DiscussionsController(IDiscussionService discussionService)
        {
            _discussionService = discussionService;
        }

        [Authorize]
        [HttpGet("lesson/{lessonId}")]
        public async Task<ActionResult<IEnumerable<DiscussionResponse>>> GetLessonDiscussions(int lessonId)
        {
            var result = await _discussionService.GetLessonDiscussionsAsync(lessonId);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<DiscussionDetailResponse>> GetDiscussionDetail(int id)
        {
            var result = await _discussionService.GetDiscussionDetailAsync(id);
            return Ok(result);
        }

        [Authorize(Roles = "Learner,Instructor")]
        [HttpPost]
        public async Task<ActionResult<DiscussionResponse>> CreateDiscussion([FromBody] CreateDiscussionRequest request)
        {
            var userId = User.GetUserId();
            var result = await _discussionService.CreateDiscussionAsync(userId, request);
            return Ok(result);
        }

        [Authorize(Roles = "Learner,Instructor")]
        [HttpPut("{id}")]
        public async Task<ActionResult<DiscussionResponse>> UpdateDiscussion(int id, [FromBody] UpdateDiscussionRequest request)
        {
            var userId = User.GetUserId();
            var result = await _discussionService.UpdateDiscussionAsync(userId, id, request);
            return Ok(result);
        }

        [Authorize(Roles = "Learner,Instructor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiscussion(int id)
        {
            var userId = User.GetUserId();
            await _discussionService.DeleteDiscussionAsync(userId, id);
            return NoContent();
        }

        [Authorize(Roles = "Learner,Instructor")]
        [HttpPost("{id}/replies")]
        public async Task<ActionResult<ReplyResponse>> AddReply(int id, [FromBody] CreateReplyRequest request)
        {
            var userId = User.GetUserId();
            var result = await _discussionService.AddReplyAsync(userId, id, request);
            return Ok(result);
        }

        [Authorize(Roles = "Learner,Instructor")]
        [HttpPut("replies/{replyId}")]
        public async Task<ActionResult<ReplyResponse>> UpdateReply(int replyId, [FromBody] UpdateReplyRequest request)
        {
            var userId = User.GetUserId();
            var result = await _discussionService.UpdateReplyAsync(userId, replyId, request);
            return Ok(result);
        }

        [Authorize(Roles = "Learner,Instructor")]
        [HttpDelete("replies/{replyId}")]
        public async Task<IActionResult> DeleteReply(int replyId)
        {
            var userId = User.GetUserId();
            await _discussionService.DeleteReplyAsync(userId, replyId);
            return NoContent();
        }

        [Authorize(Roles = "Learner,Instructor")]
        [HttpPost("{id}/like")]
        public async Task<ActionResult<int>> ToggleLike(int id)
        {
            var userId = User.GetUserId();
            var likeCount = await _discussionService.ToggleLikeAsync(userId, id);
            return Ok(likeCount);
        }
    }
}
