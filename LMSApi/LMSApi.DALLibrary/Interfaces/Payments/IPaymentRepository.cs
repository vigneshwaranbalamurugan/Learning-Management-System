using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IPaymentRepository : IRepository<int, Payments>
    {
        Task<Payments?> GetByProviderOrderIdAsync(string providerOrderId);
        Task<IEnumerable<Payments>> GetPaymentsByUserAsync(int userId);
        Task<IEnumerable<Payments>> GetPaymentsByInstructorAsync(int instructorId);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
