using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface IPlatformFeeConfigRepository
    {
        /// <summary>Get the fee config effective at the given datetime for the specified category.</summary>
        Task<PlatformFeeConfig?> GetActiveConfigAsync(FeeCategory category, DateTime at);

        /// <summary>Get all fee configs, optionally filtered by category.</summary>
        Task<IEnumerable<PlatformFeeConfig>> GetAllAsync(FeeCategory? category = null);

        /// <summary>Get the latest (currently active) config for a category.</summary>
        Task<PlatformFeeConfig?> GetCurrentAsync(FeeCategory category);

        Task AddAsync(PlatformFeeConfig config);
        Task UpdateAsync(PlatformFeeConfig config);
    }
}
