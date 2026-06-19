using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.DTOs
{
    // ── Requests ──────────────────────────────────────────────────────────────
    public class SetPlatformFeeRequest
    {
        public FeeCategory Category { get; set; }
        public FeeType FeeType { get; set; }
        /// <summary>Percentage (0-100) or flat INR amount.</summary>
        public decimal Value { get; set; }
    }

    // ── Responses ─────────────────────────────────────────────────────────────
    public class PlatformFeeResponse
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string FeeType { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Description { get; set; } = string.Empty;  // e.g. "10%" or "₹50 flat"
        public DateTime EffectiveFrom { get; set; }
        public string CreatedByAdminEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
