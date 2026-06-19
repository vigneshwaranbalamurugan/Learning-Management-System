using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using AutoMapper;
using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMSApi.API.Controllers
{
    [Authorize(Roles = "Instructor,Admin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/revenue/onboarding")]
    public class InstructorOnboardingController : ControllerBase
    {
        private readonly IInstructorOnboardingService _onboardingService;
        private readonly IMapper _mapper;

        public InstructorOnboardingController(IInstructorOnboardingService onboardingService, IMapper mapper)
        {
            _onboardingService = onboardingService;
            _mapper = mapper;
        }

        /// <summary>
        /// GET /revenue/onboarding/status
        /// Get the instructor's current onboarding status.
        /// </summary>
        [HttpGet("status")]
        public async Task<ActionResult<OnboardingStatusResponse>> GetStatus()
        {
            var instructorId = User.GetUserId();
            var status = await _onboardingService.GetOnboardingStatusAsync(instructorId);
            return Ok(status);
        }

        /// <summary>
        /// POST /revenue/onboarding/account
        /// Step 1: Create Linked Account.
        /// </summary>
        [HttpPost("account")]
        public async Task<ActionResult<LinkedAccountResponse>> CreateAccount([FromBody] CreateLinkedAccountRequest request)
        {
            var instructorId = User.GetUserId();
            try
            {
                Console.WriteLine($"Incoming request for instructorId: {instructorId}, request: {System.Text.Json.JsonSerializer.Serialize(request)}");
                var account = await _onboardingService.CreateLinkedAccountAsync(instructorId, request);
                var response = _mapper.Map<LinkedAccountResponse>(account);
                return CreatedAtAction(nameof(GetStatus), null, response);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already has"))
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(502, new { message = ex.Message });
            }
        }

        /// <summary>
        /// PUT /revenue/onboarding/account
        /// Step 1: Update Linked Account.
        /// </summary>
        [HttpPut("account")]
        public async Task<ActionResult<LinkedAccountResponse>> UpdateAccount([FromBody] UpdateLinkedAccountRequest request)
        {
            var instructorId = User.GetUserId();
            try
            {
                var account = await _onboardingService.UpdateLinkedAccountAsync(instructorId, request);
                var response = _mapper.Map<LinkedAccountResponse>(account);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(502, new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST /revenue/onboarding/stakeholder
        /// Step 2: Add Director/Stakeholder.
        /// </summary>
        [HttpPost("stakeholder")]
        public async Task<ActionResult<StakeholderResponse>> CreateStakeholder([FromBody] CreateStakeholderRequest request)
        {
            var instructorId = User.GetUserId();
            try
            {
                var stakeholder = await _onboardingService.CreateStakeholderAsync(instructorId, request);
                var response = _mapper.Map<StakeholderResponse>(stakeholder);
                return CreatedAtAction(nameof(GetStatus), null, response);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("must be created before"))
            {
                return UnprocessableEntity(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already registered"))
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(502, new { message = ex.Message });
            }
        }

        /// <summary>
        /// PUT /revenue/onboarding/stakeholder
        /// Step 2: Update Stakeholder.
        /// </summary>
        [HttpPut("stakeholder")]
        public async Task<ActionResult<StakeholderResponse>> UpdateStakeholder([FromBody] UpdateStakeholderRequest request)
        {
            var instructorId = User.GetUserId();
            try
            {
                var stakeholder = await _onboardingService.UpdateStakeholderAsync(instructorId, request);
                var response = _mapper.Map<StakeholderResponse>(stakeholder);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                return UnprocessableEntity(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(502, new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST /revenue/onboarding/product
        /// Step 3: Request Route Product.
        /// </summary>
        [HttpPost("product")]
        public async Task<ActionResult<PayoutProductResponse>> RequestProduct()
        {
            var instructorId = User.GetUserId();
            try
            {
                var product = await _onboardingService.RequestProductAsync(instructorId);
                var response = _mapper.Map<PayoutProductResponse>(product);
                return CreatedAtAction(nameof(GetStatus), null, response);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("must be created before") || ex.Message.Contains("must be registered"))
            {
                return UnprocessableEntity(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(502, new { message = ex.Message });
            }
        }

        /// <summary>
        /// PATCH /revenue/onboarding/product/bank
        /// Step 4: Configure bank details.
        /// </summary>
        [HttpPatch("product/bank")]
        public async Task<ActionResult<PayoutProductResponse>> ConfigureBank([FromBody] ConfigureBankRequest request)
        {
            var instructorId = User.GetUserId();
            try
            {
                var product = await _onboardingService.ConfigureBankAsync(instructorId, request);
                var response = _mapper.Map<PayoutProductResponse>(product);
                return Ok(response);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found") || ex.Message.Contains("must be requested"))
            {
                return UnprocessableEntity(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(502, new { message = ex.Message });
            }
        }
    }
}
