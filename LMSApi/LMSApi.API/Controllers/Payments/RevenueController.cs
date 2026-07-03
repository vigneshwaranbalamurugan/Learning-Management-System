using Asp.Versioning;
using AutoMapper;
using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
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
    [Route("api/v{version:apiVersion}/revenue")]
    public class RevenueController : ControllerBase
    {
        private readonly IInstructorPayoutService _payoutService;
        private readonly IRevenueService _revenueService;
        private readonly IMapper _mapper;

        public RevenueController(IInstructorPayoutService payoutService, IRevenueService revenueService, IMapper mapper)
        {
            _payoutService = payoutService;
            _revenueService = revenueService;
            _mapper = mapper;
        }

        // ── Instructor: Payout Account Registration ────────────────────────────

        /// <summary>Instructor: View own payout account status.</summary>
        [HttpGet("payout-account")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<PayoutAccountResponse>> GetMyPayoutAccount()
        {
            var instructorId = User.GetUserId();
            var account = await _payoutService.GetActiveAccountAsync(instructorId);
            if (account == null)
                return NotFound(new
                {
                    message = "No payout account registered. Submit your Razorpay Linked Account ID (acc_xxx)."
                });
            return Ok(_mapper.Map<PayoutAccountResponse>(account));
        }

        /// <summary>Instructor: View own revenue — all Route transfers and earnings summary.</summary>
        [HttpGet("instructor")]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult<PagedInstructorRevenueSummaryResponse>> GetMyRevenue(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var instructorId = User.GetUserId();
            var summary = await _revenueService.GetInstructorRevenueSummaryAsync(instructorId, search, status, page, pageSize);
            return Ok(summary);
        }

        // ── Admin: Revenue Dashboard ───────────────────────────────────────────

        /// <summary>Admin: Full revenue dashboard — all Route transfers across all instructors.</summary>
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("AdminApis")]
        public async Task<ActionResult<AdminRevenueResponse>> GetAdminRevenue()
        {
            var dashboard = await _revenueService.GetAdminRevenueDashboardAsync();
            return Ok(dashboard);
        }

        /// <summary>Admin: List all Route transfers needing manual intervention.</summary>
        [HttpGet("admin/pending-manual-review")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("AdminApis")]
        public async Task<ActionResult<IEnumerable<InstructorPayoutResponse>>> GetPendingManualReviews()
        {
            var payouts = await _payoutService.GetPendingManualReviewAsync();
            return Ok(_mapper.Map<IEnumerable<InstructorPayoutResponse>>(payouts));
        }

        /// <summary>Admin: Paginated, filterable list of all incoming course payments.</summary>
        [HttpGet("admin/transactions")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("AdminApis")]
        public async Task<ActionResult<PagedAdminTransactionResponse>> GetAdminTransactions(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 15)
        {
            var result = await _revenueService.GetAdminTransactionsAsync(search, status, dateFrom, dateTo, page, pageSize);
            return Ok(result);
        }

        /// <summary>Admin: Paginated, filterable list of all instructor payouts.</summary>
        [HttpGet("admin/payouts")]
        [Authorize(Roles = "Admin")]
        [EnableRateLimiting("AdminApis")]
        public async Task<ActionResult<PagedAdminPayoutResponse>> GetAdminPayouts(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 15)
        {
            var result = await _revenueService.GetAdminPayoutsAsync(search, status, dateFrom, dateTo, page, pageSize);
            return Ok(result);
        }
    }
}
