using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IPaymentRepository : IRepository<int, Payments>
    {
        Task<Payments?> GetByProviderOrderIdAsync(string providerOrderId);
        Task<IEnumerable<Payments>> GetPaymentsByUserAsync(int userId);
        Task<IEnumerable<Payments>> GetPaymentsByInstructorAsync(int instructorId);
        Task<(IEnumerable<Payments> Items, int TotalCount)> GetLearnerPaymentsPagedAsync(
            int userId, string? search, PaymentStatus? status, int page, int pageSize);
        Task<(IEnumerable<Payments> Items, int TotalCount)> GetPagedAsync(
            string? search, PaymentStatus? status, DateTime? dateFrom, DateTime? dateTo,
            int page, int pageSize);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}

