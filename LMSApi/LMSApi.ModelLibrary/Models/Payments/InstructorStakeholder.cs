using System;

namespace LMSApi.ModelLibrary.Models
{
    /// <summary>
    /// Step 2: Razorpay Route - Stakeholder creation (POST /v2/accounts/{id}/stakeholders).
    /// Represents the instructor as Director of the linked account.
    /// </summary>
    public class InstructorStakeholder
    {
        public int Id { get; set; }
        public int InstructorLinkedAccountId { get; set; }         // FK → InstructorLinkedAccount

        public string RazorpayStakeholderId { get; set; } = string.Empty;   // sth_xxx
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public InstructorLinkedAccount LinkedAccount { get; set; } = null!;
    }
}
