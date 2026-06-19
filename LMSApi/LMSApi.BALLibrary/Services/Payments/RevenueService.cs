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

        public async Task<InstructorRevenueSummaryResponse> GetInstructorRevenueSummaryAsync(int instructorId)
        {
            var instructorPayments = (await _paymentRepository.GetPaymentsByInstructorAsync(instructorId)).ToList();
            var payouts = (await _payoutService.GetPayoutsForInstructorAsync(instructorId)).ToList();

            var totalEarned = payouts
                .Where(p => p.Status == PayoutStatus.Processed)
                .Sum(p => p.Amount);

            var totalShare = instructorPayments.Sum(p => p.InstructorAmount);
            var pendingAmount = Math.Max(0, totalShare - totalEarned);

            return new InstructorRevenueSummaryResponse
            {
                InstructorId = instructorId,
                TotalEarned = totalEarned,
                PendingAmount = pendingAmount,
                TotalPayouts = payouts.Count,
                Payouts = _mapper.Map<List<InstructorPayoutResponse>>(payouts)
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
                TotalTransactions = completedPayments.Count,
                ByInstructor = byInstructor,
                PendingManualReviews = _mapper.Map<List<InstructorPayoutResponse>>(pendingManualReview)
            };
        }
    }
}
