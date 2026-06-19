using System.Threading.Tasks;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IInstructorPayoutProductRepository
    {
        Task<InstructorPayoutProduct?> GetByLinkedAccountIdAsync(int linkedAccountId);
        Task AddAsync(InstructorPayoutProduct product);
        Task UpdateAsync(InstructorPayoutProduct product);
    }
}
