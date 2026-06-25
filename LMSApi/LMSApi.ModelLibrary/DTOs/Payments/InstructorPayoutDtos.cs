using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LMSApi.ModelLibrary.DTOs
{
    // ── STEP 1: Create Linked Account ─────────────────────────────────────────
    public class CreateLinkedAccountRequest
    {
        [Required][EmailAddress][MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required][RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be 10 digits")]
        public string Phone { get; set; } = string.Empty;

        [Required][MinLength(3)][MaxLength(200)]
        public string LegalBusinessName { get; set; } = string.Empty;

        [Required][MinLength(2)][MaxLength(100)]
        public string ContactName { get; set; } = string.Empty;

        [Required]
        public string BusinessType { get; set; } = "individual";

        [Required]
        public string ProfileCategory { get; set; } = "education";

        [Required]
        public string ProfileSubcategory { get; set; } = "other_educational_services";

        [Required][MaxLength(500)]
        public string Street1 { get; set; } = string.Empty;
        public string? Street2 { get; set; }

        [Required][MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required][MaxLength(100)]
        public string State { get; set; } = string.Empty;

        [Required][RegularExpression(@"^\d{6}$", ErrorMessage = "Enter a valid 6-digit postal code")]
        public string PostalCode { get; set; } = string.Empty;

        [Required][MaxLength(2)]
        public string Country { get; set; } = "IN";

        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]$", ErrorMessage = "Enter a valid PAN")]
        public string? Pan { get; set; } = string.Empty;

        [RegularExpression(@"^\d{2}[A-Z]{5}\d{4}[A-Z]\d[Z][A-Z\d]$", ErrorMessage = "Enter a valid GSTIN")]
        public string? Gst { get; set; }
    }

    // ── STEP 1: Update Linked Account (no BusinessType — immutable post-creation) ─
    public class UpdateLinkedAccountRequest
    {
        [Required][EmailAddress][MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required][RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be 10 digits")]
        public string Phone { get; set; } = string.Empty;

        [Required][MinLength(3)][MaxLength(200)]
        public string LegalBusinessName { get; set; } = string.Empty;

        [Required][MinLength(2)][MaxLength(100)]
        public string ContactName { get; set; } = string.Empty;

        [Required] public string ProfileCategory { get; set; } = "education";
        [Required] public string ProfileSubcategory { get; set; } = "other_educational_services";

        [Required][MaxLength(500)] public string Street1 { get; set; } = string.Empty;
        public string? Street2 { get; set; }
        [Required][MaxLength(100)] public string City { get; set; } = string.Empty;
        [Required][MaxLength(100)] public string State { get; set; } = string.Empty;
        [Required][RegularExpression(@"^\d{6}$", ErrorMessage = "Enter a valid 6-digit postal code")] public string PostalCode { get; set; } = string.Empty;
        [Required][MaxLength(2)] public string Country { get; set; } = "IN";
        [RegularExpression(@"^[A-Z]{5}[0-9]{4}[A-Z]$", ErrorMessage = "Enter a valid PAN")] public string? Pan { get; set; }
        [RegularExpression(@"^\d{2}[A-Z]{5}\d{4}[A-Z]\d[Z][A-Z\d]$", ErrorMessage = "Enter a valid GSTIN")] public string? Gst { get; set; }
    }

    // ── STEP 2: Stakeholder ────────────────────────────────────────────────────
    public class CreateStakeholderRequest
    {
        [Required][MinLength(2)][MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required][EmailAddress][MaxLength(255)]
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateStakeholderRequest
    {
        [Required][MinLength(2)][MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required][EmailAddress][MaxLength(255)]
        public string Email { get; set; } = string.Empty;
    }

    // ── STEP 4: Configure Bank + Accept T&C ───────────────────────────────────
    public class ConfigureBankRequest
    {
        [Required][RegularExpression(@"^\d{9,18}$", ErrorMessage = "Enter a valid account number (9–18 digits)")]
        public string AccountNumber { get; set; } = string.Empty;

        [Required][RegularExpression(@"^[A-Z]{4}0[A-Z0-9]{6}$", ErrorMessage = "Enter a valid IFSC code")]
        public string IfscCode { get; set; } = string.Empty;

        [Required][MinLength(2)][MaxLength(100)]
        public string BeneficiaryName { get; set; } = string.Empty;
    }

    // ── Step-Split Responses ──────────────────────────────────────────────────
    public class LinkedAccountResponse
    {
        public int Id { get; set; }
        public string RazorpayAccountId { get; set; } = string.Empty;
        public string LegalBusinessName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string BusinessType { get; set; } = string.Empty;
        public string AccountStatus { get; set; } = string.Empty;
        
        public string Street1 { get; set; } = string.Empty;
        public string? Street2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = "IN";
        public string? Pan { get; set; }
        public string? Gst { get; set; }

        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public bool HasStakeholder { get; set; }
        public bool HasProduct { get; set; }
        public bool IsBankConfigured { get; set; }
    }

    public class StakeholderResponse
    {
        public int Id { get; set; }
        public string RazorpayStakeholderId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class PayoutProductResponse
    {
        public int Id { get; set; }
        public string RazorpayProductId { get; set; } = string.Empty;
        public string ProductStatus { get; set; } = string.Empty;
        public bool TncAccepted { get; set; }
        public string AccountNumber { get; set; } = string.Empty;   // Masked: ****1234
        public string IfscCode { get; set; } = string.Empty;
        public string BeneficiaryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class OnboardingStatusResponse
    {
        public string CurrentStep { get; set; } = string.Empty;     // "step1", "step2", "step3", "step4", "completed"
        public string AccountStatus { get; set; } = string.Empty;   // "created", "under_review", "activated"
        public LinkedAccountResponse? Account { get; set; }
        public StakeholderResponse? Stakeholder { get; set; }
        public PayoutProductResponse? Product { get; set; }
    }

    // ── Requests ──────────────────────────────────────────────────────────────
    /// <summary>Instructor submits this to register their Route payout account.</summary>
    public class RegisterPayoutAccountRequest
    {
        public string LegalBusinessName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string IfscCode { get; set; } = string.Empty;
        
        // Razorpay KYC & Profile requirements
        public string BusinessType { get; set; } = "individual"; // e.g., individual, partnership, private_limited
        public string ProfileCategory { get; set; } = "education";
        public string ProfileSubcategory { get; set; } = "other_educational_services";
        
        public string Street1 { get; set; } = string.Empty;
        public string? Street2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = "IN";
        
        public string Pan { get; set; } = string.Empty;
        public string? Gst { get; set; }
    }

    // ── Responses ─────────────────────────────────────────────────────────────
    public class PayoutAccountResponse
    {
        public int Id { get; set; }
        public string RazorpayLinkedAccountId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsRouteReady { get; set; }   // true if RazorpayLinkedAccountId is set
        public DateTime CreatedAt { get; set; }
    }

    public class InstructorPayoutResponse
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string? StudentName { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RazorpayTransferId { get; set; }  // Route transfer ID (trf_xxx)
        public string? FailureReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class InstructorRevenueSummaryResponse
    {
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public decimal TotalEarned { get; set; }
        public decimal PendingAmount { get; set; }
        public int TotalPayouts { get; set; }
        public List<InstructorPayoutResponse> Payouts { get; set; } = new();
    }

    public class AdminRevenueResponse
    {
        public decimal TotalRevenue { get; set; }           // Total received from students
        public decimal TotalPlatformFees { get; set; }      // Sum of platform fees retained
        public decimal TotalInstructorPayouts { get; set; } // Sum paid out to instructors (processed)
        public int TotalTransactions { get; set; }
        public List<InstructorRevenueSummaryResponse> ByInstructor { get; set; } = new();
        public List<InstructorPayoutResponse> PendingManualReviews { get; set; } = new();
    }
}
