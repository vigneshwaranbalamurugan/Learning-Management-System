using LMSApi.ModelLibrary.Enums;

namespace LMSApi.ModelLibrary.Models
{
    /// <summary>
    /// Stores each platform fee configuration as an immutable record.
    /// Each admin change creates a new row — old rows are never modified,
    /// preserving the full audit trail of fee changes.
    /// </summary>
    public class PlatformFeeConfig
    {
        public int Id { get; set; }

        /// <summary>Type of fee: CourseFee or CertificateFee (future).</summary>
        public FeeCategory FeeCategory { get; set; }

        /// <summary>Whether the fee is a percentage of the amount or a fixed flat amount.</summary>
        public FeeType FeeType { get; set; }

        /// <summary>
        /// The fee value. For Percentage type, this is the percentage (e.g. 10 = 10%).
        /// For Flat type, this is the fixed INR amount (e.g. 50 = ₹50).
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// The UTC datetime from which this fee config is effective.
        /// All payments on or after this datetime use this config.
        /// </summary>
        public DateTime EffectiveFrom { get; set; }

        /// <summary>Admin who created this fee configuration.</summary>
        public int CreatedByAdminId { get; set; }

        public DateTime CreatedAt { get; set; }

        /// <summary>Supports soft delete.</summary>
        public bool IsActive { get; set; } = true;

        // Navigation
        public Users CreatedByAdmin { get; set; } = null!;
    }
}
