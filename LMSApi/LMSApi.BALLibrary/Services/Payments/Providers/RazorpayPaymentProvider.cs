using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.Settings;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMSApi.BALLibrary.Services
{
    public class RazorpayPaymentProvider : IPaymentProvider
    {
        private readonly RazorpaySettings _settings;

        public RazorpayPaymentProvider(IConfiguration configuration)
        {
            _settings = new RazorpaySettings
            {
                KeyId = configuration["Razorpay:KeyId"] ?? string.Empty,
                KeySecret = configuration["Razorpay:KeySecret"] ?? string.Empty
            };
        }

        public string ProviderName => "Razorpay";

        public Task<string> CreateOrderAsync(decimal amount, string currency, string receiptId)
        {
            // Amount is in paisa (smallest unit) for INR
            int amountInPaisa = (int)(amount * 100);

            Dictionary<string, object> options = new Dictionary<string, object>
            {
                { "amount", amountInPaisa },
                { "currency", currency },
                { "receipt", receiptId }
            };

            RazorpayClient client = new RazorpayClient(_settings.KeyId, _settings.KeySecret);
            Order order = client.Order.Create(options);

            return Task.FromResult(order["id"].ToString());
        }

        public bool VerifySignature(string orderId, string paymentId, string signature)
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>
            {
                { "razorpay_order_id", orderId },
                { "razorpay_payment_id", paymentId },
                { "razorpay_signature", signature }
            };

            try
            {
                Razorpay.Api.Utils.verifyPaymentSignature(attributes);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
