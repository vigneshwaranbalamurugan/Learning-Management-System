using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IPaymentRepository : IRepository<int, Payments>
    {
        Task<Payments?> GetByRazorpayOrderIdAsync(string razorpayOrderId);
        Task<IEnumerable<Payments>> GetPaymentsByUserAsync(int userId);
    }
}
