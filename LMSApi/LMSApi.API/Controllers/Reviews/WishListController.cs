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
    [Authorize(Roles = "Learner")]
    public class WishListController : ControllerBase
    {
        private readonly IWishListService _wishListService;

        public WishListController(IWishListService wishListService)
        {
            _wishListService = wishListService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WishListResponse>>> GetUserWishList()
        {
            var userId = User.GetUserId();
            var result = await _wishListService.GetUserWishListAsync(userId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<WishListResponse>> AddToWishList([FromBody] AddWishListRequest request)
        {
            var userId = User.GetUserId();
            var result = await _wishListService.AddToWishListAsync(userId, request);
            return Ok(result);
        }

        [HttpDelete("{courseId}")]
        public async Task<IActionResult> RemoveFromWishList(int courseId)
        {
            var userId = User.GetUserId();
            await _wishListService.RemoveFromWishListAsync(userId, courseId);
            return NoContent();
        }
    }
}
