using System;

namespace LMSApi.ModelLibrary.Models
{
    /// <summary>
    /// Step 1: Razorpay Route - Linked Account creation (POST /v2/accounts).
    /// Stores core business identity: legal name, contact, type, address, KYC.
    /// </summary>
    public class InstructorLinkedAccount
    {
        public int Id { get; set; }
        public int InstructorId { get; set; }

        // From Razorpay API response
        public string RazorpayAccountId { get; set; } = string.Empty;   // acc_xxx

        // Business identity (Step 1 payload fields)
        public string LegalBusinessName { get; set; } = string.Empty;
        public string BusinessType { get; set; } = string.Empty;        // immutable after creation
        public string ContactName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // Business address
        public string Street1 { get; set; } = string.Empty;
        public string? Street2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = "IN";

        // KYC / legal info
        public string? Pan { get; set; }
        public string? Gst { get; set; }

        // Profile (Razorpay dashboard category)
        public string ProfileCategory { get; set; } = "education";
        public string ProfileSubcategory { get; set; } = "other_educational_services";

        // Account lifecycle
        public string AccountStatus { get; set; } = "created";          // created → under_review → activated
        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public Users Instructor { get; set; } = null!;
        public InstructorStakeholder? Stakeholder { get; set; }
        public InstructorPayoutProduct? PayoutProduct { get; set; }
    }
}
