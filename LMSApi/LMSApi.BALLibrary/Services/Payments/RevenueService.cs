using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMSApi.BALLibrary.Services
{
    public class RevenueService : IRevenueService
    {
        private readonly IInstructorPayoutService _payoutService;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;

        public RevenueService(
            IInstructorPayoutService payoutService,
            IPaymentRepository paymentRepository,
            IMapper mapper)
        {
            _payoutService = payoutService;
            _paymentRepository = paymentRepository;
            _mapper = mapper;
        }

        public async Task<PagedInstructorRevenueSummaryResponse> GetInstructorRevenueSummaryAsync(int instructorId, string? search = null, string? status = null, int page = 1, int pageSize = 10)
        {
            var instructorPayments = (await _paymentRepository.GetPaymentsByInstructorAsync(instructorId)).ToList();
            var allPayouts = (await _payoutService.GetPayoutsForInstructorAsync(instructorId)).ToList();

            var totalEarned = allPayouts
                .Where(p => p.Status == PayoutStatus.Processed)
                .Sum(p => p.Amount);

            var totalShare = instructorPayments.Sum(p => p.InstructorAmount);
            var pendingAmount = Math.Max(0, totalShare - totalEarned);

            // Filter payouts
            var filteredPayouts = allPayouts.AsEnumerable();
            
            if (!string.IsNullOrWhiteSpace(search))
            {
                filteredPayouts = filteredPayouts.Where(p => 
                    (p.Payment?.Course?.Title != null && p.Payment.Course.Title.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Payment?.User?.UserProfile?.FirstName != null && p.Payment.User.UserProfile.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Payment?.User?.UserProfile?.LastName != null && p.Payment.User.UserProfile.LastName.Contains(search, StringComparison.OrdinalIgnoreCase))
                );
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PayoutStatus>(status, true, out var parsedStatus))
            {
                filteredPayouts = filteredPayouts.Where(p => p.Status == parsedStatus);
            }

            var totalPayoutsCount = filteredPayouts.Count();

            // Paginate payouts
            var paginatedPayouts = filteredPayouts
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedInstructorRevenueSummaryResponse
            {
                InstructorId = instructorId,
                TotalEarned = totalEarned,
                PendingAmount = pendingAmount,
                TotalPayouts = totalPayoutsCount,
                Payouts = _mapper.Map<List<InstructorPayoutResponse>>(paginatedPayouts),
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalPayoutsCount / (double)pageSize)
            };
        }

        public async Task<AdminRevenueResponse> GetAdminRevenueDashboardAsync()
        {
            var allPayouts = (await _payoutService.GetAllPayoutsAsync()).ToList();
            var completedPayments = (await _paymentRepository.GetAllAsync())
                .Where(p => p.Status == PaymentStatus.Completed || p.Status == PaymentStatus.Transferred)
                .ToList();

            var totalRevenue = completedPayments.Sum(p => p.Amount);
            var totalPlatformFees = completedPayments.Sum(p => p.PlatformFeeAmount);
            var totalInstructorPayouts = allPayouts
                .Where(p => p.Status == PayoutStatus.Processed)
                .Sum(p => p.Amount);
            var totalPendingPayouts = completedPayments.Sum(p => p.InstructorAmount) - totalInstructorPayouts;

            // Group payments by instructor
            var paymentsByInstructor = completedPayments
                .GroupBy(p => p.Course.InstructorId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Group payouts by instructor
            var payoutsByInstructor = allPayouts
                .GroupBy(p => p.InstructorId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Get all unique instructor IDs from both payments and payouts
            var instructorIds = paymentsByInstructor.Keys.Union(payoutsByInstructor.Keys).ToList();

            var byInstructor = new List<InstructorRevenueSummaryResponse>();
            foreach (var instId in instructorIds)
            {
                var instPayments = paymentsByInstructor.TryGetValue(instId, out var pmts) ? pmts : new List<Payments>();
                var instPayouts = payoutsByInstructor.TryGetValue(instId, out var pyts) ? pyts : new List<InstructorPayout>();

                var totalEarned = instPayouts.Where(p => p.Status == PayoutStatus.Processed).Sum(p => p.Amount);
                var totalShare = instPayments.Sum(p => p.InstructorAmount);
                var pendingAmount = Math.Max(0, totalShare - totalEarned);

                var instructorName = "Instructor #" + instId;
                if (instPayments.Any(p => p.Course?.Instructor?.UserProfile != null))
                {
                    var userProfile = instPayments.First(p => p.Course?.Instructor?.UserProfile != null).Course.Instructor.UserProfile;
                    instructorName = $"{userProfile.FirstName} {userProfile.LastName}";
                }
                else if (instPayouts.Any(p => p.Instructor?.UserProfile != null))
                {
                    var userProfile = instPayouts.First(p => p.Instructor?.UserProfile != null).Instructor.UserProfile;
                    instructorName = $"{userProfile.FirstName} {userProfile.LastName}";
                }

                byInstructor.Add(new InstructorRevenueSummaryResponse
                {
                    InstructorId = instId,
                    InstructorName = instructorName,
                    TotalEarned = totalEarned,
                    PendingAmount = pendingAmount,
                    TotalPayouts = instPayouts.Count,
                    Payouts = _mapper.Map<List<InstructorPayoutResponse>>(instPayouts)
                });
            }

            var pendingManualReview = await _payoutService.GetPendingManualReviewAsync();

            return new AdminRevenueResponse
            {
                TotalRevenue = totalRevenue,
                TotalPlatformFees = totalPlatformFees,
                TotalInstructorPayouts = totalInstructorPayouts,
                TotalPendingPayouts = totalPendingPayouts > 0 ? totalPendingPayouts : 0,
                TotalTransactions = completedPayments.Count,
                ByInstructor = byInstructor,
                PendingManualReviews = _mapper.Map<List<InstructorPayoutResponse>>(pendingManualReview)
            };
        }

        public async Task<PagedAdminTransactionResponse> GetAdminTransactionsAsync(
            string? search, string? status, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize)
        {
            PaymentStatus? parsedStatus = null;
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PaymentStatus>(status, true, out var s))
                parsedStatus = s;

            var (items, totalCount) = await _paymentRepository.GetPagedAsync(search, parsedStatus, dateFrom, dateTo, page, pageSize);
            var paymentList = items.ToList();

            var mapped = paymentList.Select(p => new AdminTransactionResponse
            {
                Id = p.Id,
                LearnerName = p.User?.UserProfile != null
                    ? $"{p.User.UserProfile.FirstName} {p.User.UserProfile.LastName}"
                    : "Unknown",
                LearnerEmail = p.User?.Email ?? "",
                CourseName = p.Course?.Title ?? "Unknown",
                InstructorName = p.Course?.Instructor?.UserProfile != null
                    ? $"{p.Course.Instructor.UserProfile.FirstName} {p.Course.Instructor.UserProfile.LastName}"
                    : "Unknown",
                GrossAmount = p.Amount,
                PlatformFeeAmount = p.PlatformFeeAmount,
                InstructorAmount = p.InstructorAmount,
                Currency = p.Currency,
                Status = p.Status.ToString(),
                DisputeStatus = p.DisputeStatus == DisputeStatus.None ? null : p.DisputeStatus.ToString(),
                PaidAt = p.PaidAt,
                CreatedAt = p.CreatedAt,
                ProviderPaymentId = p.ProviderPaymentId
            }).ToList();

            return new PagedAdminTransactionResponse
            {
                Items = mapped,
                TotalRevenue = paymentList.Sum(p => p.Amount),
                TotalPlatformFees = paymentList.Sum(p => p.PlatformFeeAmount),
                TotalInstructorShare = paymentList.Sum(p => p.InstructorAmount),
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<PagedAdminPayoutResponse> GetAdminPayoutsAsync(
            string? search, string? status, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize)
        {
            var allPayouts = (await _payoutService.GetAllPayoutsAsync()).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                allPayouts = allPayouts.Where(p =>
                    (p.Instructor?.UserProfile?.FirstName?.ToLower().Contains(lower) ?? false) ||
                    (p.Instructor?.UserProfile?.LastName?.ToLower().Contains(lower) ?? false) ||
                    (p.Payment?.Course?.Title?.ToLower().Contains(lower) ?? false) ||
                    (p.RazorpayPayoutId?.ToLower().Contains(lower) ?? false));
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PayoutStatus>(status, true, out var ps))
                allPayouts = allPayouts.Where(p => p.Status == ps);

            if (dateFrom.HasValue)
                allPayouts = allPayouts.Where(p => p.CreatedAt >= dateFrom.Value);

            if (dateTo.HasValue)
                allPayouts = allPayouts.Where(p => p.CreatedAt <= dateTo.Value.AddDays(1));

            var list = allPayouts.OrderByDescending(p => p.CreatedAt).ToList();
            var totalCount = list.Count;
            var totalPaidOut = list.Where(p => p.Status == PayoutStatus.Processed).Sum(p => p.Amount);
            var totalPending = list.Where(p => p.Status == PayoutStatus.Pending || p.Status == PayoutStatus.PendingManualReview).Sum(p => p.Amount);

            var paged = list.Skip((page - 1) * pageSize).Take(pageSize);

            var mapped = paged.Select(p => new AdminPayoutItemResponse
            {
                Id = p.Id,
                InstructorName = p.Instructor?.UserProfile != null
                    ? $"{p.Instructor.UserProfile.FirstName} {p.Instructor.UserProfile.LastName}"
                    : "Unknown",
                InstructorEmail = p.Instructor?.Email ?? "",
                CourseName = p.Payment?.Course?.Title ?? "Unknown",
                LearnerName = p.Payment?.User?.UserProfile != null
                    ? $"{p.Payment.User.UserProfile.FirstName} {p.Payment.User.UserProfile.LastName}"
                    : null,
                Amount = p.Amount,
                Status = p.Status.ToString(),
                RazorpayTransferId = p.RazorpayPayoutId,
                FailureReason = p.FailureReason,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });

            return new PagedAdminPayoutResponse
            {
                Items = mapped,
                TotalPaidOut = totalPaidOut,
                TotalPending = totalPending,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }
    }
}
