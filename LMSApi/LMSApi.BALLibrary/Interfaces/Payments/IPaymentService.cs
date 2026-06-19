using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IPaymentService
    {
        Task<string> CreateOrderAsync(string providerName, decimal amount, string currency, string receiptId);
        bool VerifySignature(string providerName, string orderId, string paymentId, string signature);

        /// <summary>Create a Razorpay order for a course purchase (calculates split internally).</summary>
        Task<(string orderId, decimal platformFee, decimal instructorAmount, int? configId)>
            CreateCourseOrderAsync(int courseId, string currency = "INR");

        /// <summary>Verify signature and save the confirmed payment with fee snapshot.</summary>
        Task<Payments> ConfirmPaymentAsync(
            string providerOrderId,
            string providerPaymentId,
            string signature,
            int userId,
            int courseId,
            int? enrollmentId,
            int? platformFeeConfigId,
            decimal platformFeeAmount,
            decimal instructorAmount,
            ModelLibrary.Enums.FeeType? feeTypeSnapshot,
            decimal? feeValueSnapshot);
    }
}
