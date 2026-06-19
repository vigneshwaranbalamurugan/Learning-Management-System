using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.Enums;
using Microsoft.Extensions.Logging;

namespace LMSApi.BALLibrary.Services
{
    public class InstructorOnboardingService : IInstructorOnboardingService
    {
        private readonly IInstructorLinkedAccountRepository _linkedAccountRepo;
        private readonly IInstructorStakeholderRepository _stakeholderRepo;
        private readonly IInstructorPayoutProductRepository _payoutProductRepo;
        private readonly IPaymentProvider _razorpayProvider;
        private readonly IMapper _mapper;
        private readonly ILogger<InstructorOnboardingService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly IUserNotificationsService _userNotificationsService;

        public InstructorOnboardingService(
            IInstructorLinkedAccountRepository linkedAccountRepo,
            IInstructorStakeholderRepository stakeholderRepo,
            IInstructorPayoutProductRepository payoutProductRepo,
            IEnumerable<IPaymentProvider> providers,
            IMapper mapper,
            ILogger<InstructorOnboardingService> logger,
            IUserRepository userRepository,
            INotificationService notificationService,
            IUserNotificationsService userNotificationsService)
        {
            _linkedAccountRepo = linkedAccountRepo;
            _stakeholderRepo = stakeholderRepo;
            _payoutProductRepo = payoutProductRepo;
            _mapper = mapper;
            _logger = logger;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _userNotificationsService = userNotificationsService;
            _razorpayProvider = providers.First(p => p.ProviderName.Equals("Razorpay", StringComparison.OrdinalIgnoreCase));
        }

        private async Task NotifyInstructorAsync(int instructorId, string subject, string emailBody, string title, string realtimeMessage)
        {
            try
            {
                var instructorUser = await _userRepository.GetByIdAsync(instructorId);
                if (instructorUser != null)
                {
                    // Format email with HTML template
                    var name = instructorUser.UserProfile?.FirstName ?? instructorUser.Email;
                    var htmlBody = Utils.EmailTemplate.GetPayoutOnboardingTemplate(name, subject, emailBody);

                    // Send Email
                    try
                    {
                        var msg = new EmailMessage(instructorUser.Email, subject, htmlBody) { IsHtml = true };
                        await _notificationService.Send(msg);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send email notification to {Email}", instructorUser.Email);
                    }

                    // Send Realtime Notification
                    try
                    {
                        await _userNotificationsService.CreateAndSendNotificationAsync(instructorId, title, realtimeMessage, NotificationType.General);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send realtime notification to user {UserId}", instructorId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch instructor user {UserId} for sending notifications", instructorId);
            }
        }

        // Step 1: Create Linked Account
        public async Task<InstructorLinkedAccount> CreateLinkedAccountAsync(int instructorId, CreateLinkedAccountRequest request)
        {
            var existing = await _linkedAccountRepo.GetActiveByInstructorIdAsync(instructorId);
            if (existing != null)
            {
                throw new InvalidOperationException("Instructor already has an active linked account.");
            }

            try
            {
                string razorpayAccountId = await _razorpayProvider.CreateLinkedAccountOnlyAsync(
                    request.Email,
                    request.Phone,
                    request.LegalBusinessName,
                    request.ContactName,
                    request.BusinessType,
                    request.ProfileCategory,
                    request.ProfileSubcategory,
                    request.Street1,
                    request.Street2,
                    request.City,
                    request.State,
                    request.PostalCode,
                    request.Country,
                    request.Pan,
                    request.Gst,
                    instructorId.ToString()
                );

                var account = new InstructorLinkedAccount
                {
                    InstructorId = instructorId,
                    RazorpayAccountId = razorpayAccountId,
                    LegalBusinessName = request.LegalBusinessName,
                    BusinessType = request.BusinessType,
                    ContactName = request.ContactName,
                    Email = request.Email,
                    Phone = request.Phone,
                    Street1 = request.Street1,
                    Street2 = request.Street2,
                    City = request.City,
                    State = request.State,
                    PostalCode = request.PostalCode,
                    Country = request.Country,
                    Pan = request.Pan,
                    Gst = request.Gst,
                    ProfileCategory = request.ProfileCategory,
                    ProfileSubcategory = request.ProfileSubcategory,
                    AccountStatus = "created",
                    IsActive = true,
                    IsVerified = false
                };

                await _linkedAccountRepo.AddAsync(account);

                await NotifyInstructorAsync(
                    instructorId,
                    "Payout Onboarding: Linked Account Created",
                    $"Dear Instructor, your Razorpay Route Linked Account has been created successfully with Account ID: {razorpayAccountId}.",
                    "Payout Account Created",
                    $"Your Razorpay Route Linked Account has been created successfully with Account ID: {razorpayAccountId}."
                );

                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating linked account for instructor {InstructorId}", instructorId);
                throw new InvalidOperationException($"Failed to create Razorpay linked account: {ex.Message}", ex);
            }
        }

        // Step 1 (Update)
        public async Task<InstructorLinkedAccount> UpdateLinkedAccountAsync(int instructorId, UpdateLinkedAccountRequest request)
        {
            var account = await _linkedAccountRepo.GetActiveByInstructorIdAsync(instructorId);
            if (account == null)
            {
                throw new KeyNotFoundException("No active linked account found for update.");
            }

            try
            {
                await _razorpayProvider.UpdateLinkedAccountOnlyAsync(
                    account.RazorpayAccountId,
                    request.Email,
                    request.Phone,
                    request.LegalBusinessName,
                    request.ContactName,
                    request.ProfileCategory,
                    request.ProfileSubcategory,
                    request.Street1,
                    request.Street2,
                    request.City,
                    request.State,
                    request.PostalCode,
                    request.Country,
                    request.Pan,
                    request.Gst
                );

                account.Email = request.Email;
                account.Phone = request.Phone;
                account.LegalBusinessName = request.LegalBusinessName;
                account.ContactName = request.ContactName;
                account.ProfileCategory = request.ProfileCategory;
                account.ProfileSubcategory = request.ProfileSubcategory;
                account.Street1 = request.Street1;
                account.Street2 = request.Street2;
                account.City = request.City;
                account.State = request.State;
                account.PostalCode = request.PostalCode;
                account.Country = request.Country;
                account.Pan = request.Pan;
                account.Gst = request.Gst;

                await _linkedAccountRepo.UpdateAsync(account);

                await NotifyInstructorAsync(
                    instructorId,
                    "Payout Onboarding: Linked Account Updated",
                    "Dear Instructor, your Razorpay Route Linked Account details have been updated successfully.",
                    "Payout Account Updated",
                    "Your Razorpay Route Linked Account details have been updated."
                );

                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating linked account {AccountId} for instructor {InstructorId}", account.RazorpayAccountId, instructorId);
                throw new InvalidOperationException($"Failed to update Razorpay linked account: {ex.Message}", ex);
            }
        }

        // Step 2: Create Stakeholder
        public async Task<InstructorStakeholder> CreateStakeholderAsync(int instructorId, CreateStakeholderRequest request)
        {
            var account = await _linkedAccountRepo.GetActiveByInstructorIdAsync(instructorId);
            if (account == null)
            {
                throw new InvalidOperationException("Linked account (Step 1) must be created before adding a stakeholder.");
            }

            var existing = await _stakeholderRepo.GetByLinkedAccountIdAsync(account.Id);
            if (existing != null)
            {
                throw new InvalidOperationException("Stakeholder already registered for this account.");
            }

            try
            {
                string stakeholderId = await _razorpayProvider.CreateStakeholderOnlyAsync(
                    account.RazorpayAccountId,
                    request.Name,
                    request.Email
                );

                var stakeholder = new InstructorStakeholder
                {
                    InstructorLinkedAccountId = account.Id,
                    RazorpayStakeholderId = stakeholderId,
                    Name = request.Name,
                    Email = request.Email
                };

                await _stakeholderRepo.AddAsync(stakeholder);

                await NotifyInstructorAsync(
                    instructorId,
                    "Payout Onboarding: Stakeholder Added",
                    $"Dear Instructor, a stakeholder ({request.Name}) has been successfully added to your linked account.",
                    "Stakeholder Added",
                    $"Stakeholder {request.Name} has been added to your payout account."
                );

                return stakeholder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating stakeholder for linked account {AccountId}", account.RazorpayAccountId);
                throw new InvalidOperationException($"Failed to create Razorpay stakeholder: {ex.Message}", ex);
            }
        }

        // Step 2 (Update)
        public async Task<InstructorStakeholder> UpdateStakeholderAsync(int instructorId, UpdateStakeholderRequest request)
        {
            var account = await _linkedAccountRepo.GetActiveByInstructorIdAsync(instructorId);
            if (account == null)
            {
                throw new InvalidOperationException("Linked account not found.");
            }

            var stakeholder = await _stakeholderRepo.GetByLinkedAccountIdAsync(account.Id);
            if (stakeholder == null)
            {
                throw new KeyNotFoundException("No stakeholder found to update.");
            }

            try
            {
                await _razorpayProvider.UpdateStakeholderOnlyAsync(
                    account.RazorpayAccountId,
                    stakeholder.RazorpayStakeholderId,
                    request.Name,
                    request.Email
                );

                stakeholder.Name = request.Name;
                stakeholder.Email = request.Email;

                await _stakeholderRepo.UpdateAsync(stakeholder);

                await NotifyInstructorAsync(
                    instructorId,
                    "Payout Onboarding: Stakeholder Updated",
                    $"Dear Instructor, stakeholder details for {request.Name} have been updated successfully.",
                    "Stakeholder Updated",
                    $"Stakeholder details for {request.Name} have been updated."
                );

                return stakeholder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stakeholder {StakeholderId} for account {AccountId}", stakeholder.RazorpayStakeholderId, account.RazorpayAccountId);
                throw new InvalidOperationException($"Failed to update Razorpay stakeholder: {ex.Message}", ex);
            }
        }

        // Step 3: Request Route Product
        public async Task<InstructorPayoutProduct> RequestProductAsync(int instructorId)
        {
            var account = await _linkedAccountRepo.GetActiveByInstructorIdAsync(instructorId);
            if (account == null)
            {
                throw new InvalidOperationException("Linked account (Step 1) must be created before requesting a product.");
            }

            var stakeholder = await _stakeholderRepo.GetByLinkedAccountIdAsync(account.Id);
            if (stakeholder == null)
            {
                throw new InvalidOperationException("Stakeholder (Step 2) must be created before requesting a product.");
            }

            var existing = await _payoutProductRepo.GetByLinkedAccountIdAsync(account.Id);
            if (existing != null)
            {
                throw new InvalidOperationException("Product configuration already exists/requested.");
            }

            try
            {
                string productId = await _razorpayProvider.CreateProductConfigurationOnlyAsync(
                    account.RazorpayAccountId,
                    "route"
                );

                var product = new InstructorPayoutProduct
                {
                    InstructorLinkedAccountId = account.Id,
                    RazorpayProductId = productId,
                    ProductStatus = "requested",
                    TncAccepted = false
                };

                await _payoutProductRepo.AddAsync(product);

                await NotifyInstructorAsync(
                    instructorId,
                    "Payout Onboarding: Product Requested",
                    "Dear Instructor, the Route payout product has been requested for your account.",
                    "Product Requested",
                    "Route payout product has been requested."
                );

                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting product for account {AccountId}", account.RazorpayAccountId);
                throw new InvalidOperationException($"Failed to request product from Razorpay: {ex.Message}", ex);
            }
        }

        // Step 4: Configure Bank settles
        public async Task<InstructorPayoutProduct> ConfigureBankAsync(int instructorId, ConfigureBankRequest request)
        {
            var account = await _linkedAccountRepo.GetActiveByInstructorIdAsync(instructorId);
            if (account == null)
            {
                throw new InvalidOperationException("Linked account not found.");
            }

            var product = await _payoutProductRepo.GetByLinkedAccountIdAsync(account.Id);
            if (product == null)
            {
                throw new InvalidOperationException("Product (Step 3) must be requested before bank configuration.");
            }

            try
            {
                await _razorpayProvider.UpdateProductConfigurationOnlyAsync(
                    account.RazorpayAccountId,
                    product.RazorpayProductId,
                    request.AccountNumber,
                    request.IfscCode,
                    request.BeneficiaryName
                );

                product.AccountNumber = request.AccountNumber;
                product.IfscCode = request.IfscCode;
                product.BeneficiaryName = request.BeneficiaryName;
                product.TncAccepted = true;
                product.ProductStatus = "active";

                await _payoutProductRepo.UpdateAsync(product);

                string maskedAccount = request.AccountNumber.Length > 4
                    ? "****" + request.AccountNumber.Substring(request.AccountNumber.Length - 4)
                    : request.AccountNumber;

                await NotifyInstructorAsync(
                    instructorId,
                    "Payout Onboarding: Bank Configured",
                    $"Dear Instructor, settlement bank account details ending in {maskedAccount} have been configured for your linked account.",
                    "Bank Details Configured",
                    "Settlement bank details have been configured."
                );

                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configuring bank details for product {ProductId} on account {AccountId}", product.RazorpayProductId, account.RazorpayAccountId);
                throw new InvalidOperationException($"Failed to configure bank details: {ex.Message}", ex);
            }
        }

        // Get Onboarding Status
        public async Task<OnboardingStatusResponse> GetOnboardingStatusAsync(int instructorId)
        {
            var account = await _linkedAccountRepo.GetActiveByInstructorIdAsync(instructorId);
            if (account == null)
            {
                return new OnboardingStatusResponse
                {
                    CurrentStep = "step1",
                    AccountStatus = "not_started"
                };
            }

            var response = new OnboardingStatusResponse
            {
                AccountStatus = account.AccountStatus,
                Account = _mapper.Map<LinkedAccountResponse>(account)
            };

            if (account.Stakeholder != null)
            {
                response.Stakeholder = _mapper.Map<StakeholderResponse>(account.Stakeholder);
            }

            if (account.PayoutProduct != null)
            {
                response.Product = _mapper.Map<PayoutProductResponse>(account.PayoutProduct);
            }

            // Determine current step
            if (account.Stakeholder == null)
            {
                response.CurrentStep = "step2";
            }
            else if (account.PayoutProduct == null)
            {
                response.CurrentStep = "step3";
            }
            else if (!account.PayoutProduct.TncAccepted)
            {
                response.CurrentStep = "step4";
            }
            else
            {
                response.CurrentStep = "completed";
            }

            return response;
        }

        // Webhook handler
        public async Task HandleAccountWebhookAsync(string razorpayAccountId, string eventType)
        {
            var account = await _linkedAccountRepo.GetByRazorpayAccountIdAsync(razorpayAccountId);
            if (account == null)
            {
                _logger.LogWarning("Linked account {RazorpayAccountId} not found in DB for webhook {Event}", razorpayAccountId, eventType);
                return;
            }

            _logger.LogInformation("Processing webhook {Event} for Linked Account {RazorpayAccountId}", eventType, razorpayAccountId);

            string subject = "";
            string emailBody = "";
            string title = "";
            string realtimeMessage = "";

            if (eventType.Equals("account.activated", StringComparison.OrdinalIgnoreCase))
            {
                account.AccountStatus = "activated";
                account.IsVerified = true;
                account.VerifiedAt = DateTime.UtcNow;

                subject = "Payout Onboarding: Account Activated!";
                emailBody = "Dear Instructor, congratulations! Your Razorpay Route payout account has been fully activated. You are now ready to receive split payouts automatically.";
                title = "Payout Account Activated";
                realtimeMessage = "Your payout account has been fully activated and verified.";
            }
            else if (eventType.Equals("account.under_review", StringComparison.OrdinalIgnoreCase))
            {
                account.AccountStatus = "under_review";

                subject = "Payout Onboarding: Account Under Review";
                emailBody = "Dear Instructor, your Razorpay Route payout account is now under review. We will notify you once the verification is complete.";
                title = "Payout Account Under Review";
                realtimeMessage = "Your payout account is currently under review.";
            }
            else if (eventType.Equals("account.suspended", StringComparison.OrdinalIgnoreCase))
            {
                account.AccountStatus = "suspended";
                account.IsActive = false;

                subject = "Payout Onboarding: Account Suspended";
                emailBody = "Dear Instructor, your Razorpay Route payout account has been suspended. Please contact support.";
                title = "Payout Account Suspended";
                realtimeMessage = "Your payout account has been suspended.";
            }

            await _linkedAccountRepo.UpdateAsync(account);

            if (!string.IsNullOrEmpty(subject))
            {
                await NotifyInstructorAsync(account.InstructorId, subject, emailBody, title, realtimeMessage);
            }
        }
    }
}
