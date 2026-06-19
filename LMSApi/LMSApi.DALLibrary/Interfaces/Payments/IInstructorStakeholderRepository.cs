using System.Threading.Tasks;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IInstructorStakeholderRepository
    {
        Task<InstructorStakeholder?> GetByLinkedAccountIdAsync(int linkedAccountId);
        Task AddAsync(InstructorStakeholder stakeholder);
        Task UpdateAsync(InstructorStakeholder stakeholder);
    }
}
