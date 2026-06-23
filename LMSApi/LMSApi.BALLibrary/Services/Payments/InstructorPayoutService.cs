using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.DTOs;
using Microsoft.Extensions.Logging;

namespace LMSApi.BALLibrary.Services
{
    public class InstructorPayoutService : IInstructorPayoutService
    {
        private readonly IInstructorPayoutRepository _payoutRepo;
        private readonly IInstructorPayoutAccountRepository _accountRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly IPaymentProvider _razorpayProvider;
        private readonly ILogger<InstructorPayoutService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUserNotificationsService _userNotificationsService;

        public InstructorPayoutService(
            IInstructorPayoutRepository payoutRepo,
            IInstructorPayoutAccountRepository accountRepo,
            IPaymentRepository paymentRepo,
            IEnumerable<IPaymentProvider> providers,
            ILogger<InstructorPayoutService> logger,
            INotificationService notificationService,
            IUserRepository userRepository,
            ICourseRepository courseRepository,
            IUserNotificationsService userNotificationsService)
        {
            _payoutRepo = payoutRepo;
            _accountRepo = accountRepo;
            _paymentRepo = paymentRepo;
            _logger = logger;
            _notificationService = notificationService;
            _userRepository = userRepository;
            _courseRepository = courseRepository;
            _userNotificationsService = userNotificationsService;
            _razorpayProvider = providers.First(p =>
                p.ProviderName.Equals("Razorpay", StringComparison.OrdinalIgnoreCase));
        }

        // ── Instructor: Account Registration ──────────────────────────────────
        public async Task<InstructorPayoutAccount> RegisterPayoutAccountAsync(
            int instructorId,
            RegisterPayoutAccountRequest request)
        {
            // Check for existing active account BEFORE hitting Razorpay API
            var existingAccount = await _accountRepo.GetActiveByInstructorIdAsync(instructorId);
            if (existingAccount != null)
            {
                throw new InvalidOperationException("Instructor already has an active payout account registered. Please use the update endpoint.");
            }

            // We call Razorpay API BEFORE starting the DB transaction, because the API call could be slow or fail.
            // 1. Create Linked Account (and Stakeholder, Product, Activation)
            var result = await _razorpayProvider.CreateLinkedAccountAsync(
                request.Email, request.Phone, request.LegalBusinessName, request.ContactName, request.BusinessType,
                request.ProfileCategory, request.ProfileSubcategory,
                request.Street1, request.Street2, request.City, request.State, request.PostalCode, request.Country,
                request.Pan, request.Gst, request.AccountNumber, request.IfscCode);

            await _accountRepo.BeginTransactionAsync();
            try
            {

                var account = new InstructorPayoutAccount
                {
                    InstructorId = instructorId,
                    RazorpayLinkedAccountId = result.AccountId,
                    RazorpayStakeholderId = result.StakeholderId,
                    RazorpayProductId = result.ProductId,
                    AccountStatus = "under_review",
                    LegalBusinessName = request.LegalBusinessName,
                    ContactName = request.ContactName,
                    Email = request.Email,
                    Phone = request.Phone,
                    AccountNumber = request.AccountNumber,
                    IfscCode = request.IfscCode,
                    BusinessType = request.BusinessType,
                    ProfileCategory = request.ProfileCategory,
                    ProfileSubcategory = request.ProfileSubcategory,
                    Street1 = request.Street1,
                    Street2 = request.Street2,
                    City = request.City,
                    State = request.State,
                    PostalCode = request.PostalCode,
                    Country = request.Country,
                    Pan = request.Pan,
                    Gst = request.Gst,
                    IsActive = true
                };

                await _accountRepo.AddAsync(account);
                await _accountRepo.CommitTransactionAsync();

                _logger.LogInformation(
                    "Instructor {InstructorId} registered payout account {AccountId} (acc: {LinkedAccountId})",
                    instructorId, account.Id, result.AccountId);

                return account;
            }
            catch (Exception)
            {
                await _accountRepo.RollbackTransactionAsync();
                throw;
            }
        }


        public async Task<InstructorPayoutAccount> UpdatePayoutAccountAsync(
            int instructorId,
            RegisterPayoutAccountRequest request)
        {
            var existing = await _accountRepo.GetActiveByInstructorIdAsync(instructorId);
            if (existing == null)
            {
                throw new InvalidOperationException("No active payout account found to update.");
            }

            // Update Razorpay side (all components internally via provider)
            await _razorpayProvider.UpdateLinkedAccountAsync(
                existing.RazorpayLinkedAccountId,
                existing.RazorpayStakeholderId,
                existing.RazorpayProductId,
                request.Email, request.Phone, request.LegalBusinessName, request.ContactName,
                request.ProfileCategory, request.ProfileSubcategory,
                request.Street1, request.Street2, request.City, request.State, request.PostalCode, request.Country, 
                request.Pan, request.Gst, request.AccountNumber, request.IfscCode);

            // Update local DB
            existing.LegalBusinessName = request.LegalBusinessName;
            existing.ContactName = request.ContactName;
            existing.Email = request.Email;
            existing.Phone = request.Phone;
            existing.AccountNumber = request.AccountNumber;
            existing.IfscCode = request.IfscCode;
            existing.BusinessType = request.BusinessType;
            existing.ProfileCategory = request.ProfileCategory;
            existing.ProfileSubcategory = request.ProfileSubcategory;
            existing.Street1 = request.Street1;
            existing.Street2 = request.Street2;
            existing.City = request.City;
            existing.State = request.State;
            existing.PostalCode = request.PostalCode;
            existing.Country = request.Country;
            existing.Pan = request.Pan;
            existing.Gst = request.Gst;

            await _accountRepo.UpdateAsync(existing);

            _logger.LogInformation("Instructor {InstructorId} updated payout account {AccountId}", instructorId, existing.Id);

            return existing;
        }

        public async Task<InstructorPayoutAccount?> GetActiveAccountAsync(int instructorId)
            => await _accountRepo.GetActiveByInstructorIdAsync(instructorId);

        // ── Route Transfer ────────────────────────────────────────────────────
        public async Task<InstructorPayout> InitiatePayoutAsync(Payments payment, int instructorId)
        {
            var account = await _accountRepo.GetActiveByInstructorIdAsync(instructorId);
            if (account == null)
            {
                throw new InvalidOperationException($"Instructor {instructorId} has no payout account registered.");
            }

            if (string.IsNullOrEmpty(payment.ProviderPaymentId))
                throw new InvalidOperationException(
                    $"Cannot initiate Route transfer: payment {payment.Id} has no Razorpay Payment ID (pay_xxx).");

            var payout = new InstructorPayout
            {
                PaymentId = payment.Id,
                InstructorId = instructorId,
                InstructorPayoutAccountId = account.Id,
                Amount = payment.InstructorAmount,
                RazorpayFundAccountId = account.RazorpayLinkedAccountId,
                Status = PayoutStatus.Pending,
                Notes = $"Route transfer for Razorpay payment {payment.ProviderPaymentId}"
            };

            await _payoutRepo.AddAsync(payout);

            try
            {
                _logger.LogInformation(
                    "Initiating Route transfer of ₹{Amount} to linked account {AccId} (payment: {PayId})",
                    payout.Amount, account.RazorpayLinkedAccountId, payment.ProviderPaymentId);

                // CreatePayoutAsync is repurposed for Route: fundAccountId = linked acc_xxx, purpose = payment ID
                var transferId = await _razorpayProvider.CreatePayoutAsync(
                    fundAccountId: account.RazorpayLinkedAccountId,
                    amount: payout.Amount,
                    currency: "INR",
                    purpose: payment.ProviderPaymentId!,
                    narration: null);

                payout.RazorpayPayoutId = transferId;
                payout.Status = PayoutStatus.Processing;
                await _payoutRepo.UpdateAsync(payout);

                payment.Status = PaymentStatus.Transferred;
                await _paymentRepo.UpdateAsync(payment);

                _logger.LogInformation(
                    "Route transfer {TransferId} initiated for instructor {InstructorId}",
                    transferId, instructorId);

                // ── Send Payout Initiated Email ──
                var instructorUser = await _userRepository.GetByIdAsync(instructorId);
                var course = await _courseRepository.GetByIdAsync(payment.CourseId);
                var html = Utils.EmailTemplate.GetInstructorPayoutTemplate(
                    instructorUser.UserProfile?.FirstName ?? instructorUser.Email,
                    course.Title,
                    payout.Amount, payout.RazorpayPayoutId);
                Message msg = new EmailMessage(instructorUser.Email, "Payout Initiated!", html) { IsHtml = true };
                await _notificationService.Send(msg);

                try
                {
                    await _userNotificationsService.CreateAndSendNotificationAsync(
                        userId: instructorId,
                        title: "Payout Initiated",
                        message: $"A payout of ₹{payout.Amount} has been initiated for '{course.Title}'.",
                        type: NotificationType.General);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send payout initiated realtime notification to Instructor {InstructorId}", instructorId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to initiate Route transfer for payment {PaymentId}, instructor {InstructorId}",
                    payment.Id, instructorId);

                payout.Status = PayoutStatus.PendingManualReview;
                payout.FailureReason = $"Route transfer initiation failed: {ex.Message}";
                await _payoutRepo.UpdateAsync(payout);
            }

            return payout;
        }

        public async Task HandleWebhookAsync(string razorpayTransferId, string eventType, string? failureReason = null)
        {
            var payout = await _payoutRepo.GetByRazorpayPayoutIdAsync(razorpayTransferId);
            if (payout == null)
            {
                _logger.LogWarning("Webhook for unknown transfer/payout ID: {Id}", razorpayTransferId);
                return;
            }

            switch (eventType.ToLowerInvariant())
            {
                case "transfer.processed":
                case "payout.processed":
                    payout.Status = PayoutStatus.Processed;
                    _logger.LogInformation("Transfer {Id} processed successfully", razorpayTransferId);

                    var processedInstructor = await _userRepository.GetByIdAsync(payout.InstructorId);
                    var processedHtml = Utils.EmailTemplate.GetPayoutWebhookTemplate(
                        processedInstructor.UserProfile?.FirstName ?? processedInstructor.Email,
                        "Processed successfully", payout.RazorpayPayoutId ?? razorpayTransferId, null);
                    Message processedMsg = new EmailMessage(processedInstructor.Email, "Payout Processed", processedHtml) { IsHtml = true };
                    await _notificationService.Send(processedMsg);

                    try
                    {
                        await _userNotificationsService.CreateAndSendNotificationAsync(
                            userId: payout.InstructorId,
                            title: "Payout Processed",
                            message: $"Your payout of ₹{payout.Amount} has been processed successfully.",
                            type: NotificationType.General);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send payout processed realtime notification to Instructor {InstructorId}", payout.InstructorId);
                    }
                    break;

                case "transfer.failed":
                case "payout.failed":
                    payout.Status = PayoutStatus.PendingManualReview;
                    payout.FailureReason = failureReason ?? "Transfer failed (no reason provided)";
                    _logger.LogWarning("Transfer {Id} FAILED → PendingManualReview. Reason: {R}",
                        razorpayTransferId, payout.FailureReason);

                    var failedInstructor = await _userRepository.GetByIdAsync(payout.InstructorId);
                    var failedHtml = Utils.EmailTemplate.GetPayoutWebhookTemplate(
                        failedInstructor.UserProfile?.FirstName ?? failedInstructor.Email,
                        "Failed", payout.RazorpayPayoutId ?? razorpayTransferId, payout.FailureReason);
                    Message failedMsg = new EmailMessage(failedInstructor.Email, "Payout Failed", failedHtml) { IsHtml = true };
                    await _notificationService.Send(failedMsg);

                    try
                    {
                        await _userNotificationsService.CreateAndSendNotificationAsync(
                            userId: payout.InstructorId,
                            title: "Payout Failed",
                            message: $"Your payout of ₹{payout.Amount} failed. Reason: {payout.FailureReason}",
                            type: NotificationType.General);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send payout failed realtime notification to Instructor {InstructorId}", payout.InstructorId);
                    }
                    break;

                case "transfer.reversed":
                case "payout.reversed":
                    payout.Status = PayoutStatus.PendingManualReview;
                    payout.FailureReason = failureReason ?? "Transfer reversed by bank";
                    _logger.LogWarning("Transfer {Id} REVERSED → PendingManualReview.", razorpayTransferId);
                    break;

                case "payout.queued":
                    payout.Status = PayoutStatus.Queued;
                    break;

                default:
                    _logger.LogInformation("Unhandled webhook event: {Event}", eventType);
                    return;
            }

            await _payoutRepo.UpdateAsync(payout);
        }

        // ── Revenue Reporting ──────────────────────────────────────────────────
        public async Task<IEnumerable<InstructorPayout>> GetPayoutsForInstructorAsync(int instructorId)
            => await _payoutRepo.GetByInstructorAsync(instructorId);

        public async Task<IEnumerable<InstructorPayout>> GetAllPayoutsAsync()
            => await _payoutRepo.GetAllAsync();

        public async Task<IEnumerable<InstructorPayout>> GetPendingManualReviewAsync()
            => await _payoutRepo.GetByStatusAsync(PayoutStatus.PendingManualReview);
    }
}
