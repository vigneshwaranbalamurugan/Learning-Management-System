using System.Threading.Tasks;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IInvoiceService
    {
        Task<byte[]> GenerateInvoiceAsync(int paymentId);
    }
}
