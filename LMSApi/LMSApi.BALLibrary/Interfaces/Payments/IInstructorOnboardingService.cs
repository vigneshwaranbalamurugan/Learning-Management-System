using System.Threading.Tasks;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface IInstructorOnboardingService
    {
        // Step 1
        Task<InstructorLinkedAccount> CreateLinkedAccountAsync(int instructorId, CreateLinkedAccountRequest request);
        Task<InstructorLinkedAccount> UpdateLinkedAccountAsync(int instructorId, UpdateLinkedAccountRequest request);

        // Step 2
        Task<InstructorStakeholder> CreateStakeholderAsync(int instructorId, CreateStakeholderRequest request);
        Task<InstructorStakeholder> UpdateStakeholderAsync(int instructorId, UpdateStakeholderRequest request);

        // Step 3
        Task<InstructorPayoutProduct> RequestProductAsync(int instructorId);

        // Step 4
        Task<InstructorPayoutProduct> ConfigureBankAsync(int instructorId, ConfigureBankRequest request);

        // Status
        Task<OnboardingStatusResponse> GetOnboardingStatusAsync(int instructorId);

        // Webhook handler — account status update
        Task HandleAccountWebhookAsync(string razorpayAccountId, string eventType);
    }
}
