using System;

namespace LMSApi.ModelLibrary.Models
{
    /// <summary>
    /// Steps 3 & 4: Razorpay Route - Product configuration.
    /// Step 3: POST /v2/accounts/{id}/products (request Route product).
    /// Step 4: PATCH /v2/accounts/{id}/products/{productId} (configure bank + accept T&C).
    /// </summary>
    public class InstructorPayoutProduct
    {
        public int Id { get; set; }
        public int InstructorLinkedAccountId { get; set; }         // FK → InstructorLinkedAccount

        public string RazorpayProductId { get; set; } = string.Empty;  // prod_xxx

        // Bank settlement details (Step 4)
        public string AccountNumber { get; set; } = string.Empty;
        public string IfscCode { get; set; } = string.Empty;
        public string BeneficiaryName { get; set; } = string.Empty;
        public bool TncAccepted { get; set; } = false;

        // Product activation status
        public string ProductStatus { get; set; } = "requested";   // requested → activated

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public InstructorLinkedAccount LinkedAccount { get; set; } = null!;
    }
}
