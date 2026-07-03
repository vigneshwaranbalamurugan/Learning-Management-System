using System.Threading.Tasks;
using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IRevenueService
    {
        /// <summary>Get own revenue summary for the specified instructor.</summary>
        Task<PagedInstructorRevenueSummaryResponse> GetInstructorRevenueSummaryAsync(int instructorId, string? search = null, string? status = null, int page = 1, int pageSize = 10);

        /// <summary>Get the full admin revenue dashboard KPI totals.</summary>
        Task<AdminRevenueResponse> GetAdminRevenueDashboardAsync();

        /// <summary>Admin: Paginated list of all incoming course payments with filtering.</summary>
        Task<PagedAdminTransactionResponse> GetAdminTransactionsAsync(string? search, string? status, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize);

        /// <summary>Admin: Paginated list of all instructor payouts with filtering.</summary>
        Task<PagedAdminPayoutResponse> GetAdminPayoutsAsync(string? search, string? status, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize);
    }
}
