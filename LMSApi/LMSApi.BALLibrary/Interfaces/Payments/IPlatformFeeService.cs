using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IPlatformFeeService
    {
        /// <summary>Set a new fee configuration (admin only). Previous configs are preserved.</summary>
        Task<PlatformFeeConfig> SetFeeAsync(FeeCategory category, FeeType feeType, decimal value, int adminId);

        /// <summary>Update an existing active fee configuration for a category.</summary>
        Task<PlatformFeeConfig> UpdateFeeAsync(FeeCategory category, FeeType feeType, decimal value, int adminId);

        /// <summary>Get the currently active fee config for a category.</summary>
        Task<PlatformFeeConfig?> GetCurrentFeeAsync(FeeCategory category);

        /// <summary>Get full history of fee changes, optionally filtered by category.</summary>
        Task<IEnumerable<PlatformFeeConfig>> GetFeeHistoryAsync(FeeCategory? category = null);

        /// <summary>
        /// Calculate the platform fee and instructor amount for a given total.
        /// Uses the fee config effective at the specified datetime (or now if null).
        /// Returns (platformFeeAmount, instructorAmount, configUsed).
        /// </summary>
        Task<(decimal platformFeeAmount, decimal instructorAmount, PlatformFeeConfig? configUsed)>
            CalculateSplitAsync(decimal totalAmount, FeeCategory category, DateTime? at = null);
    }
}
