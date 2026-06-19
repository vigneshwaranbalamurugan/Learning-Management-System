namespace LMSApi.ModelLibrary.Models
{
    /// <summary>
    /// Stores instructor's Razorpay Linked Account (acc_xxx) for Route transfers.
    /// Instructor submits their acc_xxx and payouts are made directly.
    /// </summary>
    public class InstructorPayoutAccount
    {
        public int Id { get; set; }
        public int InstructorId { get; set; }

        /// <summary>Razorpay Linked Account ID (acc_xxx) for Route transfers.</summary>
        public string RazorpayLinkedAccountId { get; set; } = string.Empty;

        public string LegalBusinessName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string IfscCode { get; set; } = string.Empty;

        // Razorpay KYC Details
        public string BusinessType { get; set; } = string.Empty;
        public string ProfileCategory { get; set; } = string.Empty;
        public string ProfileSubcategory { get; set; } = string.Empty;
        public string Street1 { get; set; } = string.Empty;
        public string? Street2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Pan { get; set; } = string.Empty;
        public string? Gst { get; set; }

        /// <summary>Only one active account per instructor at a time.</summary>
        public bool IsActive { get; set; } = true;

        // Razorpay Full Onboarding State
        public string? RazorpayStakeholderId { get; set; }
        public string? RazorpayProductId { get; set; }
        public string AccountStatus { get; set; } = "created"; // "created", "under_review", "activated"
        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public Users Instructor { get; set; } = null!;
    }
}
