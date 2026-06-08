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
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<ReviewResponse>>> GetCourseReviews(int courseId)
        {
            var result = await _reviewService.GetCourseReviewsAsync(courseId);
            return Ok(result);
        }

        [Authorize(Roles = "Learner")]
        [HttpPost]
        public async Task<ActionResult<ReviewResponse>> AddReview([FromBody] CreateReviewRequest request)
        {
            var userId = User.GetUserId();
            var result = await _reviewService.AddReviewAsync(userId, request);
            return Ok(result);
        }

        [Authorize(Roles = "Learner")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ReviewResponse>> UpdateReview(int id, [FromBody] UpdateReviewRequest request)
        {
            var userId = User.GetUserId();
            var result = await _reviewService.UpdateReviewAsync(userId, id, request);
            return Ok(result);
        }

        [Authorize(Roles = "Learner")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var userId = User.GetUserId();
            await _reviewService.DeleteReviewAsync(userId, id);
            return NoContent();
        }
    }
}
