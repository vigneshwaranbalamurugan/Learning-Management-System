using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.Settings;
using Microsoft.Extensions.Configuration;

namespace LMSApi.BALLibrary.Services
{
    public class StripePaymentProvider : IPaymentProvider
    {
        private readonly StripeSettings _settings;

        public StripePaymentProvider(IConfiguration configuration)
        {
            _settings = new StripeSettings
            {
                SecretKey = configuration["Stripe:SecretKey"] ?? string.Empty,
                PublishableKey = configuration["Stripe:PublishableKey"] ?? string.Empty
            };
        }

        public string ProviderName => "Stripe";

        public Task<string> CreateOrderAsync(decimal amount, string currency, string receiptId)
        {
            // Placeholder for Stripe payment intent creation
            string fakeIntentId = "pi_mock_" + Guid.NewGuid().ToString("N")[..10];
            return Task.FromResult(fakeIntentId);
        }

        public bool VerifySignature(string orderId, string paymentId, string signature)
        {
            // Placeholder for Stripe webhook signature verification
            return true;
        }

        // ── Payout methods not supported for Stripe (Razorpay-only feature) ──
        public Task<string> CreateOrGetContactAsync(string name, string email, string contactType = "vendor")
            => throw new NotSupportedException("Payouts via bank transfer are only supported for Razorpay.");

        public Task<string> CreateFundAccountAsync(string contactId, string accountHolderName, string accountNumber, string ifscCode)
            => throw new NotSupportedException("Payouts via bank transfer are only supported for Razorpay.");

        public Task<string> CreatePayoutAsync(string fundAccountId, decimal amount, string currency, string purpose, string? narration = null)
            => throw new NotSupportedException("Payouts via bank transfer are only supported for Razorpay.");

        public bool VerifyWebhookSignature(string payload, string signature, string secret)
            => throw new NotSupportedException("Webhook verification is only supported for Razorpay.");

        public Task<LinkedAccountResult> CreateLinkedAccountAsync(
            string email,
            string phone,
            string legalBusinessName,
            string contactName,
            string businessType,
            string profileCategory,
            string profileSubcategory,
            string street1,
            string? street2,
            string city,
            string state,
            string postalCode,
            string country,
            string pan,
            string? gst,
            string accountNumber,
            string ifscCode)
            => throw new NotSupportedException("Route Linked Accounts are only supported for Razorpay.");

        public Task UpdateLinkedAccountAsync(
            string accountId,
            string? stakeholderId,
            string? productId,
            string email,
            string phone,
            string legalBusinessName,
            string contactName,
            string profileCategory,
            string profileSubcategory,
            string street1,
            string? street2,
            string city,
            string state,
            string postalCode,
            string country,
            string pan,
            string? gst,
            string accountNumber,
            string ifscCode)
            => throw new NotSupportedException("Route Linked Accounts are only supported for Razorpay.");

        public Task<string> CreateLinkedAccountOnlyAsync(
            string email,
            string phone,
            string legalBusinessName,
            string contactName,
            string businessType,
            string profileCategory,
            string profileSubcategory,
            string street1,
            string? street2,
            string city,
            string state,
            string postalCode,
            string country,
            string pan,
            string? gst,
            string referenceId)
            => throw new NotSupportedException("Route Linked Accounts are only supported for Razorpay.");

        public Task UpdateLinkedAccountOnlyAsync(
            string accountId,
            string email,
            string phone,
            string legalBusinessName,
            string contactName,
            string profileCategory,
            string profileSubcategory,
            string street1,
            string? street2,
            string city,
            string state,
            string postalCode,
            string country,
            string pan,
            string? gst)
            => throw new NotSupportedException("Route Linked Accounts are only supported for Razorpay.");

        public Task<string> CreateStakeholderOnlyAsync(string accountId, string name, string email)
            => throw new NotSupportedException("Route Linked Accounts are only supported for Razorpay.");

        public Task UpdateStakeholderOnlyAsync(string accountId, string stakeholderId, string name, string email)
            => throw new NotSupportedException("Route Linked Accounts are only supported for Razorpay.");

        public Task<string> CreateProductConfigurationOnlyAsync(string accountId, string productName)
            => throw new NotSupportedException("Route Linked Accounts are only supported for Razorpay.");

        public Task UpdateProductConfigurationOnlyAsync(string accountId, string productId, string accountNumber, string ifscCode, string beneficiaryName)
            => throw new NotSupportedException("Route Linked Accounts are only supported for Razorpay.");
    }
}
