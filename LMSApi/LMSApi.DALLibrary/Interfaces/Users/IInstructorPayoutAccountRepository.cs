using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IInstructorPayoutAccountRepository
    {
        Task<InstructorPayoutAccount?> GetActiveByInstructorIdAsync(int instructorId);
        Task<IEnumerable<InstructorPayoutAccount>> GetAllByInstructorIdAsync(int instructorId);
        Task AddAsync(InstructorPayoutAccount account);
        Task UpdateAsync(InstructorPayoutAccount account);
        Task<InstructorPayoutAccount?> GetByIdAsync(int id);

        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
