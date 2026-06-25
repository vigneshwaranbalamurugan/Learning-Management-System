using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Utils;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;

namespace LMSApi.BALLibrary.Services
{
    /// <summary>
    /// Handles cross-cutting Razorpay webhook events:
    ///   - settlement.processed         → log + notify admin users (email + real-time)
    ///   - product.route.*              → update InstructorPayoutProduct + notify instructor
    ///   - payment.downtime.*           → log only (per product decision)
    /// </summary>
    public class WebhookEventService : IWebhookEventService
    {
        private readonly IUserRepository _userRepository;
        private readonly IInstructorLinkedAccountRepository _linkedAccountRepo;
        private readonly IInstructorPayoutProductRepository _payoutProductRepo;
        private readonly INotificationService _notificationService;
        private readonly IUserNotificationsService _userNotificationsService;
        private readonly IWebhookEventLogRepository _webhookLogRepo;
        private readonly ILogger<WebhookEventService> _logger;

        public WebhookEventService(
            IUserRepository userRepository,
            IInstructorLinkedAccountRepository linkedAccountRepo,
            IInstructorPayoutProductRepository payoutProductRepo,
            INotificationService notificationService,
            IUserNotificationsService userNotificationsService,
            IWebhookEventLogRepository webhookLogRepo,
            ILogger<WebhookEventService> logger)
        {
            _userRepository = userRepository;
            _linkedAccountRepo = linkedAccountRepo;
            _payoutProductRepo = payoutProductRepo;
            _notificationService = notificationService;
            _userNotificationsService = userNotificationsService;
            _webhookLogRepo = webhookLogRepo;
            _logger = logger;
        }

        // ── settlement.processed ───────────────────────────────────────────────
        public async Task HandleSettlementAsync(string settlementId, string rawPayload)
        {
            _logger.LogInformation("Processing settlement.processed event for Settlement ID: {SettlementId}", settlementId);

            // Log the raw event for audit/idempotency
            await _webhookLogRepo.AddAsync(new WebhookEventLog
            {
                EventType = "settlement.processed",
                EntityId = settlementId,
                RawPayload = rawPayload,
                ReceivedAt = DateTime.UtcNow,
                Processed = true
            });

            // Notify all admin users via email + real-time
            try
            {
                var admins = await _userRepository.GetAdminUsersAsync();
                foreach (var admin in admins)
                {
                    var adminName = admin.UserProfile?.FirstName ?? admin.Email;

                    // Email
                    try
                    {
                        var html = EmailTemplate.GetSettlementTemplate(
                            adminName,
                            settlementId,
                            $"A Razorpay settlement with ID {settlementId} has been processed to the platform bank account.");
                        Message msg = new EmailMessage(admin.Email, "Settlement Processed", html) { IsHtml = true };
                        await _notificationService.Send(msg);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send settlement email to admin {AdminId}", admin.Id);
                    }

                    // Real-time
                    try
                    {
                        await _userNotificationsService.CreateAndSendNotificationAsync(
                            userId: admin.Id,
                            title: "Settlement Processed",
                            message: $"Razorpay settlement {settlementId} has been processed to the platform bank account.",
                            type: NotificationType.Settlement);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send settlement real-time notification to admin {AdminId}", admin.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch admin users for settlement notification.");
            }
        }

        // ── product.route.* ────────────────────────────────────────────────────
        public async Task HandleProductRouteAsync(string razorpayAccountId, string eventType)
        {
            _logger.LogInformation("Processing {EventType} for Razorpay Account: {AccountId}", eventType, razorpayAccountId);

            // Look up the linked account to find the instructor
            var account = await _linkedAccountRepo.GetByRazorpayAccountIdAsync(razorpayAccountId);
            if (account == null)
            {
                _logger.LogWarning("product.route webhook: Linked account {AccountId} not found in DB for event {Event}",
                    razorpayAccountId, eventType);
                return;
            }

            // Look up the product record
            var product = await _payoutProductRepo.GetByLinkedAccountIdAsync(account.Id);
            if (product == null)
            {
                _logger.LogWarning("product.route webhook: No product record found for linked account {AccountId}", account.Id);
                return;
            }

            string subject;
            string emailMessage;
            string notifTitle;
            string notifMessage;

            switch (eventType.ToLowerInvariant())
            {
                case "product.route.under_review":
                    product.ProductStatus = "under_review";
                    subject = "Payout Product: Under Review";
                    emailMessage = "Your Razorpay Route payout product is now under review. We will notify you once it is approved.";
                    notifTitle = "Payout Product Under Review";
                    notifMessage = "Your Route payout product is currently under review by Razorpay.";
                    break;

                case "product.route.activated":
                    product.ProductStatus = "activated";
                    subject = "Payout Product: Activated!";
                    emailMessage = "Congratulations! Your Razorpay Route payout product has been activated. You can now receive split payouts.";
                    notifTitle = "Payout Product Activated";
                    notifMessage = "Your Route payout product has been activated. Payouts are now enabled.";
                    break;

                case "product.route.needs_clarification":
                    product.ProductStatus = "needs_clarification";
                    subject = "Payout Product: Needs Clarification";
                    emailMessage = "Razorpay requires additional clarification for your Route payout product. Please log in to the Razorpay dashboard and provide the requested information.";
                    notifTitle = "Payout Product Needs Clarification";
                    notifMessage = "Razorpay needs clarification on your Route payout product. Please check your Razorpay dashboard.";
                    break;

                case "product.route.rejected":
                    product.ProductStatus = "rejected";
                    subject = "Payout Product: Rejected";
                    emailMessage = "Unfortunately, your Razorpay Route payout product application has been rejected. Please contact our support team for assistance.";
                    notifTitle = "Payout Product Rejected";
                    notifMessage = "Your Route payout product application was rejected. Please contact support.";
                    break;

                default:
                    _logger.LogWarning("Unhandled product.route event: {EventType}", eventType);
                    return;
            }

            // Update product status in DB
            await _payoutProductRepo.UpdateAsync(product);
            _logger.LogInformation("Updated product status to '{Status}' for linked account {AccountId}",
                product.ProductStatus, account.RazorpayAccountId);

            // Notify the instructor
            try
            {
                var instructor = await _userRepository.GetByIdAsync(account.InstructorId);
                var instructorName = instructor.UserProfile?.FirstName ?? instructor.Email;

                // Email
                try
                {
                    var html = EmailTemplate.GetProductRouteTemplate(instructorName, eventType, emailMessage);
                    Message msg = new EmailMessage(instructor.Email, subject, html) { IsHtml = true };
                    await _notificationService.Send(msg);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send product route email to instructor {InstructorId}", account.InstructorId);
                }

                // Real-time
                try
                {
                    await _userNotificationsService.CreateAndSendNotificationAsync(
                        userId: account.InstructorId,
                        title: notifTitle,
                        message: notifMessage,
                        type: NotificationType.ProductRoute);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send product route real-time notification to instructor {InstructorId}", account.InstructorId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify instructor {InstructorId} for product.route event {EventType}",
                    account.InstructorId, eventType);
            }
        }

        // ── payment.downtime.* ────────────────────────────────────────────────
        public async Task HandlePaymentDowntimeAsync(string eventType, string rawPayload)
        {
            // Per product decision: log only, no user notifications for downtime events
            _logger.LogWarning("Razorpay payment downtime event received: {EventType}", eventType);

            await _webhookLogRepo.AddAsync(new WebhookEventLog
            {
                EventType = eventType,
                EntityId = null,
                RawPayload = rawPayload,
                ReceivedAt = DateTime.UtcNow,
                Processed = true
            });

            _logger.LogInformation("payment.downtime event '{EventType}' logged to WebhookEventLog.", eventType);
        }
    }
}
