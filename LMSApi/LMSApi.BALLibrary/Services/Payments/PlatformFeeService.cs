using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Services
{
    public class PlatformFeeService : IPlatformFeeService
    {
        private readonly IPlatformFeeConfigRepository _feeRepo;

        public PlatformFeeService(IPlatformFeeConfigRepository feeRepo)
        {
            _feeRepo = feeRepo;
        }

        public async Task<PlatformFeeConfig> SetFeeAsync(
            FeeCategory category, FeeType feeType, decimal value, int adminId)
        {
            if (feeType == FeeType.Percentage && (value < 0 || value > 100))
                throw new ArgumentException("Percentage value must be between 0 and 100.");

            if (feeType == FeeType.Flat && value < 0)
                throw new ArgumentException("Flat fee cannot be negative.");

            var currentFee = await _feeRepo.GetCurrentAsync(category);
            if (currentFee != null)
            {
                throw new InvalidOperationException("A platform fee for this category already exists. Please use the update endpoint.");
            }

            var config = new PlatformFeeConfig
            {
                FeeCategory = category,
                FeeType = feeType,
                Value = value,
                EffectiveFrom = DateTime.UtcNow,
                CreatedByAdminId = adminId
            };

            await _feeRepo.AddAsync(config);
            return config;
        }

        public async Task<PlatformFeeConfig> UpdateFeeAsync(
            FeeCategory category, FeeType feeType, decimal value, int adminId)
        {
            if (feeType == FeeType.Percentage && (value < 0 || value > 100))
                throw new ArgumentException("Percentage value must be between 0 and 100.");

            if (feeType == FeeType.Flat && value < 0)
                throw new ArgumentException("Flat fee cannot be negative.");

            var currentFee = await _feeRepo.GetCurrentAsync(category);
            if (currentFee == null)
            {
                throw new InvalidOperationException("No active platform fee found for this category to update. Please use the create endpoint.");
            }

            currentFee.FeeType = feeType;
            currentFee.Value = value;
            currentFee.EffectiveFrom = DateTime.UtcNow;
            currentFee.CreatedByAdminId = adminId;

            await _feeRepo.UpdateAsync(currentFee);
            return currentFee;
        }

        public async Task<PlatformFeeConfig?> GetCurrentFeeAsync(FeeCategory category)
        {
            return await _feeRepo.GetCurrentAsync(category);
        }

        public async Task<IEnumerable<PlatformFeeConfig>> GetFeeHistoryAsync(FeeCategory? category = null)
        {
            return await _feeRepo.GetAllAsync(category);
        }

        public async Task<(decimal platformFeeAmount, decimal instructorAmount, PlatformFeeConfig? configUsed)>
            CalculateSplitAsync(decimal totalAmount, FeeCategory category, DateTime? at = null)
        {
            var effectiveAt = at ?? DateTime.UtcNow;
            var config = await _feeRepo.GetActiveConfigAsync(category, effectiveAt);

            if (totalAmount <= 0)
                return (0, 0, null);

            if (config == null)
            {
                // Fall back to default 10% platform fee if no config is registered
                var defaultPlatformFee = Math.Round(totalAmount * 10m / 100m, 2);
                var defaultInstructorAmount = totalAmount - defaultPlatformFee;
                return (defaultPlatformFee, defaultInstructorAmount, null);
            }

            decimal platformFee;

            if (config.FeeType == FeeType.Percentage)
            {
                platformFee = Math.Round(totalAmount * config.Value / 100, 2);
            }
            else // Flat
            {
                platformFee = Math.Min(config.Value, totalAmount); // never exceed total
            }

            var instructorAmount = totalAmount - platformFee;
            return (platformFee, instructorAmount, config);
        }
    }
}
