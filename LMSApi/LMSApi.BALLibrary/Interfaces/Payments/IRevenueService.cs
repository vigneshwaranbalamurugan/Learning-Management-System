using System.Threading.Tasks;
using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IRevenueService
    {
        /// <summary>
        /// Get own revenue summary for the specified instructor, including earnings, pending amounts, and payout history.
        /// </summary>
        Task<InstructorRevenueSummaryResponse> GetInstructorRevenueSummaryAsync(int instructorId);

        /// <summary>
        /// Get the full admin revenue dashboard, detailing total revenue, platform fees, payouts, and instructor summaries.
        /// </summary>
        Task<AdminRevenueResponse> GetAdminRevenueDashboardAsync();
    }
}
