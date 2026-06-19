namespace LMSApi.BALLibrary.Interfaces
{
    public class LinkedAccountResult
    {
        public string AccountId { get; set; } = string.Empty;
        public string StakeholderId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
    }

    public interface IPaymentProvider
    {
        string ProviderName { get; }
        Task<string> CreateOrderAsync(decimal amount, string currency, string receiptId);
        bool VerifySignature(string orderId, string paymentId, string signature);

        // ── Razorpay Route (Linked Accounts) ──────────────────────────────────────
        /// <summary>
        /// Creates a complete Linked Account including Stakeholder and Product Configuration.
        /// </summary>
        Task<LinkedAccountResult> CreateLinkedAccountAsync(
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
            string ifscCode);

        /// <summary>
        /// Updates a complete Linked Account including its Stakeholder and Product Configuration.
        /// </summary>
        Task UpdateLinkedAccountAsync(
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
            string ifscCode);

        // ── Razorpay Route Step-by-Step Onboarding ─────────────────────────────
        Task<string> CreateLinkedAccountOnlyAsync(
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
            string referenceId);

        Task UpdateLinkedAccountOnlyAsync(
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
            string? gst);

        Task<string> CreateStakeholderOnlyAsync(string accountId, string name, string email);

        Task UpdateStakeholderOnlyAsync(string accountId, string stakeholderId, string name, string email);

        Task<string> CreateProductConfigurationOnlyAsync(string accountId, string productName);

        Task UpdateProductConfigurationOnlyAsync(string accountId, string productId, string accountNumber, string ifscCode, string beneficiaryName);

        // ── Razorpay Payouts (bank transfer via Razorpay X) ───────────────────
        /// <summary>Create or retrieve a Razorpay Contact for an instructor.</summary>
        Task<string> CreateOrGetContactAsync(string name, string email, string contactType = "vendor");

        /// <summary>Create a Fund Account (bank account) under a Contact.</summary>
        Task<string> CreateFundAccountAsync(string contactId, string accountHolderName, string accountNumber, string ifscCode);

        /// <summary>Initiate a payout to a fund account from the Razorpay balance.</summary>
        Task<string> CreatePayoutAsync(string fundAccountId, decimal amount, string currency, string purpose, string? narration = null);

        /// <summary>Verify a webhook event signature from Razorpay.</summary>
        bool VerifyWebhookSignature(string payload, string signature, string secret);
    }
}
