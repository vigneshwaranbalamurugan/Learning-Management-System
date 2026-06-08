namespace LMSApi.BALLibrary.Interfaces
{
    public interface IPaymentProvider
    {
        string ProviderName { get; }
        Task<string> CreateOrderAsync(decimal amount, string currency, string receiptId);
        bool VerifySignature(string orderId, string paymentId, string signature);
    }
}
