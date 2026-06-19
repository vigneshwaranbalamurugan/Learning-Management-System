using System.Threading.Tasks;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IInstructorLinkedAccountRepository
    {
        Task<InstructorLinkedAccount?> GetActiveByInstructorIdAsync(int instructorId);
        Task<InstructorLinkedAccount?> GetByIdAsync(int id);
        Task<InstructorLinkedAccount?> GetByRazorpayAccountIdAsync(string razorpayAccountId);
        Task AddAsync(InstructorLinkedAccount account);
        Task UpdateAsync(InstructorLinkedAccount account);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
