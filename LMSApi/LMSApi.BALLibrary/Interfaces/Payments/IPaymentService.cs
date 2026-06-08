namespace LMSApi.BALLibrary.Interfaces
{
    public interface IPaymentService
    {
        Task<string> CreateOrderAsync(string providerName, decimal amount, string currency, string receiptId);
        bool VerifySignature(string providerName, string orderId, string paymentId, string signature);
    }
}
