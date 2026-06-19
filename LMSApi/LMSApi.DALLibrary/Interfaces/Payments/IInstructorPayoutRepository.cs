using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IInstructorPayoutRepository
    {
        Task AddAsync(InstructorPayout payout);
        Task UpdateAsync(InstructorPayout payout);
        Task<InstructorPayout?> GetByIdAsync(int id);
        Task<InstructorPayout?> GetByRazorpayPayoutIdAsync(string razorpayPayoutId);
        Task<IEnumerable<InstructorPayout>> GetByInstructorAsync(int instructorId);
        Task<IEnumerable<InstructorPayout>> GetAllAsync();
        Task<IEnumerable<InstructorPayout>> GetByStatusAsync(PayoutStatus status);
        Task<decimal> GetTotalEarningsAsync(int instructorId);
    }
}
