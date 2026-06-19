using Asp.Versioning;
using AutoMapper;
using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace LMSApi.API.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/platform-fees")]
    [EnableRateLimiting("AdminApis")]
    public class PlatformFeeController : ControllerBase
    {
        private readonly IPlatformFeeService _feeService;
        private readonly IMapper _mapper;

        public PlatformFeeController(IPlatformFeeService feeService, IMapper mapper)
        {
            _feeService = feeService;
            _mapper = mapper;
        }

        /// <summary>Admin: Set a new platform fee configuration (appends — old configs preserved).</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PlatformFeeResponse>> SetFee([FromBody] SetPlatformFeeRequest request)
        {
            var adminId = User.GetUserId();
            var config = await _feeService.SetFeeAsync(request.Category, request.FeeType, request.Value, adminId);
            return CreatedAtAction(nameof(GetCurrentFee), new { category = request.Category }, _mapper.Map<PlatformFeeResponse>(config));
        }

        /// <summary>Admin: Update an existing platform fee configuration.</summary>
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PlatformFeeResponse>> UpdateFee([FromBody] SetPlatformFeeRequest request)
        {
            var adminId = User.GetUserId();
            var config = await _feeService.UpdateFeeAsync(request.Category, request.FeeType, request.Value, adminId);
            return Ok(_mapper.Map<PlatformFeeResponse>(config));
        }

        /// <summary>Get the currently active fee for a category.</summary>
        [HttpGet("current")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<ActionResult<PlatformFeeResponse>> GetCurrentFee([FromQuery] FeeCategory category = FeeCategory.CourseFee)
        {
            var config = await _feeService.GetCurrentFeeAsync(category);
            if (config == null)
                return Ok(new { message = "No fee configured yet for this category.", category = category.ToString() });
            return Ok(_mapper.Map<PlatformFeeResponse>(config));
        }

        /// <summary>Admin: Full history of all fee configuration changes.</summary>
        [HttpGet("history")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<PlatformFeeResponse>>> GetHistory(
            [FromQuery] FeeCategory? category = null)
        {
            var configs = await _feeService.GetFeeHistoryAsync(category);
            return Ok(_mapper.Map<IEnumerable<PlatformFeeResponse>>(configs));
        }
    }
}
