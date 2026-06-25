using Asp.Versioning;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace LMSApi.API.Controllers
{
    /// <summary>
    /// Receives Razorpay webhook events for all event types defined in Prompt.yaml.
    /// Configure in Razorpay Dashboard: Webhooks → Add Webhook → URL: /api/v1/webhooks/razorpay/payments
    /// Active events: all order, settlement, transfer, account, product, payment events.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/webhooks/razorpay")]
    public class RazorpayWebhookController : ControllerBase
    {
        private readonly IInstructorPayoutService _payoutService;
        private readonly IEnumerable<IPaymentProvider> _providers;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RazorpayWebhookController> _logger;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IInstructorOnboardingService _onboardingService;
        private readonly IWebhookEventService _webhookEventService;
        private readonly IWebhookEventLogRepository _webhookLogRepo;

        public RazorpayWebhookController(
            IInstructorPayoutService payoutService,
            IEnumerable<IPaymentProvider> providers,
            IConfiguration configuration,
            ILogger<RazorpayWebhookController> logger,
            IEnrollmentService enrollmentService,
            IInstructorOnboardingService onboardingService,
            IWebhookEventService webhookEventService,
            IWebhookEventLogRepository webhookLogRepo)
        {
            _payoutService = payoutService;
            _providers = providers;
            _configuration = configuration;
            _logger = logger;
            _enrollmentService = enrollmentService;
            _onboardingService = onboardingService;
            _webhookEventService = webhookEventService;
            _webhookLogRepo = webhookLogRepo;
        }

        [HttpPost("payments")]
        public async Task<IActionResult> HandlePaymentWebhook()
        {
            string payload = string.Empty;
            string eventType = string.Empty;

            try
            {
                // Read raw body
                Request.EnableBuffering();
                using var reader = new System.IO.StreamReader(Request.Body, leaveOpen: true);
                payload = await reader.ReadToEndAsync();
                Request.Body.Position = 0;

                // Get signature from header (check both case variants)
                string? signature = null;
                if (Request.Headers.TryGetValue("x-razorpay-signature", out var signatureHeader))
                {
                    signature = signatureHeader.ToString();
                }
                else if (Request.Headers.TryGetValue("X-Razorpay-Signature", out var sigHeaderAlt))
                {
                    signature = sigHeaderAlt.ToString();
                }

                var secret = _configuration["Razorpay:WebhookSecret"] ?? string.Empty;
                var provider = _providers.FirstOrDefault(p => p.ProviderName.Equals("Razorpay", StringComparison.OrdinalIgnoreCase));

                // Verify signature if secret is configured
                if (!string.IsNullOrEmpty(secret))
                {
                    if (string.IsNullOrEmpty(signature))
                    {
                        _logger.LogWarning("Razorpay webhook received without signature but WebhookSecret is configured.");
                        return BadRequest("Missing signature");
                    }

                    if (provider != null && !provider.VerifyWebhookSignature(payload, signature, secret))
                    {
                        _logger.LogWarning("Razorpay webhook signature verification failed.");
                        return BadRequest("Invalid signature");
                    }
                }

                // Parse payload
                using var jsonDocument = JsonDocument.Parse(payload);
                var root = jsonDocument.RootElement;

                eventType = root.TryGetProperty("event", out var evt) ? evt.GetString() ?? string.Empty : string.Empty;
                _logger.LogInformation("Received Razorpay webhook event: {EventType}", eventType);

                if (string.IsNullOrEmpty(eventType))
                {
                    return BadRequest("Missing event type");
                }

                // ── Payout / Transfer events ─────────────────────────────────────────────────
                if (eventType.StartsWith("payout.", StringComparison.OrdinalIgnoreCase) ||
                    eventType.StartsWith("transfer.", StringComparison.OrdinalIgnoreCase))
                {
                    string? payoutId = null;
                    string? failureReason = null;

                    if (root.TryGetProperty("payload", out var payloadObj))
                    {
                        string propertyName = eventType.StartsWith("transfer.", StringComparison.OrdinalIgnoreCase) ? "transfer" : "payout";
                        
                        if (payloadObj.TryGetProperty(propertyName, out var obj) &&
                            obj.TryGetProperty("entity", out var entity))
                        {
                            payoutId = entity.TryGetProperty("id", out var idVal) ? idVal.GetString() : null;

                            if (entity.TryGetProperty("failure_reason", out var fr))
                                failureReason = fr.GetString();

                            if (entity.TryGetProperty("error", out var err) &&
                                err.TryGetProperty("description", out var desc))
                                failureReason = desc.GetString() ?? failureReason;
                        }
                    }

                    if (!string.IsNullOrEmpty(payoutId))
                    {
                        await _payoutService.HandleWebhookAsync(payoutId, eventType, failureReason);
                        _logger.LogInformation("Processed payout/transfer webhook: {Event} for {PayoutId}", eventType, payoutId);
                    }
                    else
                    {
                        _logger.LogWarning("Payout/transfer webhook missing payout ID for event: {Event}", eventType);
                    }
                }
                // ── Account events ─────────────────────────────────────────────────────────
                else if (eventType.StartsWith("account.", StringComparison.OrdinalIgnoreCase))
                {
                    string? accountId = null;
                    if (root.TryGetProperty("payload", out var pl) &&
                        pl.TryGetProperty("account", out var acct) &&
                        acct.TryGetProperty("entity", out var acctEntity))
                    {
                        accountId = acctEntity.TryGetProperty("id", out var idVal) ? idVal.GetString() : null;
                    }

                    if (!string.IsNullOrEmpty(accountId))
                    {
                        await _onboardingService.HandleAccountWebhookAsync(accountId, eventType);
                        _logger.LogInformation("Processed account webhook: {Event} for {AccountId}", eventType, accountId);
                    }
                    else
                    {
                        _logger.LogWarning("Account webhook missing account ID for event: {Event}", eventType);
                    }
                }
                // ── Product route events ───────────────────────────────────────────────
                else if (eventType.StartsWith("product.route.", StringComparison.OrdinalIgnoreCase))
                {
                    string? accountId = null;
                    if (root.TryGetProperty("payload", out var pl) &&
                        pl.TryGetProperty("account", out var acct) &&
                        acct.TryGetProperty("entity", out var acctEntity))
                    {
                        accountId = acctEntity.TryGetProperty("id", out var idVal) ? idVal.GetString() : null;
                    }

                    if (!string.IsNullOrEmpty(accountId))
                    {
                        await _webhookEventService.HandleProductRouteAsync(accountId, eventType);
                        _logger.LogInformation("Processed product.route webhook: {Event} for account {AccountId}", eventType, accountId);
                    }
                    else
                    {
                        _logger.LogWarning("product.route webhook missing account ID for event: {Event}", eventType);
                    }
                }
                // ── Settlement events ───────────────────────────────────────────────────
                else if (eventType.StartsWith("settlement.", StringComparison.OrdinalIgnoreCase))
                {
                    string? settlementId = null;
                    if (root.TryGetProperty("payload", out var pl) &&
                        pl.TryGetProperty("settlement", out var settlement) &&
                        settlement.TryGetProperty("entity", out var settlementEntity))
                    {
                        settlementId = settlementEntity.TryGetProperty("id", out var idVal) ? idVal.GetString() : null;
                    }

                    var sid = settlementId ?? $"unknown-{DateTime.UtcNow.Ticks}";
                    await _webhookEventService.HandleSettlementAsync(sid, payload);
                    _logger.LogInformation("Processed settlement webhook: {Event} for settlement {SettlementId}", eventType, sid);
                }
                // ── Payment downtime events ───────────────────────────────────────────
                else if (eventType.StartsWith("payment.downtime.", StringComparison.OrdinalIgnoreCase))
                {
                    await _webhookEventService.HandlePaymentDowntimeAsync(eventType, payload);
                }
                // ── Payment / Order events ──────────────────────────────────────────────
                else
                {
                    // Handle payment/order events (including disputes and order.notification.*)
                    string? orderId = null;
                    string? paymentId = null;

                    if (root.TryGetProperty("payload", out var payloadData))
                    {
                        // Try to get order entity first
                        if (payloadData.TryGetProperty("order", out var orderObj) &&
                            orderObj.TryGetProperty("entity", out var orderEntity))
                        {
                            orderId = orderEntity.TryGetProperty("id", out var orderIdVal) ? orderIdVal.GetString() : null;
                        }

                        // Try to get payment entity
                        if (payloadData.TryGetProperty("payment", out var paymentObj) &&
                            paymentObj.TryGetProperty("entity", out var paymentEntity))
                        {
                            paymentId = paymentEntity.TryGetProperty("id", out var paymentIdVal) ? paymentIdVal.GetString() : null;

                            // If orderId is still null, extract from payment entity
                            if (string.IsNullOrEmpty(orderId))
                            {
                                orderId = paymentEntity.TryGetProperty("order_id", out var orderIdVal) ? orderIdVal.GetString() : null;
                            }

                            // Extract dispute ID if present (for payment.dispute.* events)
                            if (eventType.StartsWith("payment.dispute.", StringComparison.OrdinalIgnoreCase))
                            {
                                // dispute ID is under payload.dispute.entity.id
                            }
                        }

                        // Extract dispute entity ID for dispute events
                        if (eventType.StartsWith("payment.dispute.", StringComparison.OrdinalIgnoreCase) &&
                            payloadData.TryGetProperty("dispute", out var disputeObj) &&
                            disputeObj.TryGetProperty("entity", out var disputeEntity))
                        {
                            // disputeId stored in payment record via service
                            _ = disputeEntity.TryGetProperty("id", out var _);
                        }
                    }

                    if (!string.IsNullOrEmpty(orderId))
                    {
                        _logger.LogInformation("Processing payment/order webhook: Event={Event}, OrderId={OrderId}, PaymentId={PaymentId}",
                            eventType, orderId, paymentId);
                        await _enrollmentService.ProcessWebhookPaymentAsync(orderId, paymentId ?? string.Empty, eventType, payload);
                    }
                    else
                    {
                        _logger.LogWarning("Payment/order webhook event {Event} missing order ID.", eventType);
                    }
                }

                // Always return 200 OK so Razorpay knows we received it
                return Ok(new { received = true, event_type = eventType });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Razorpay webhook event {EventType}.", eventType);

                // Log the failed event for retry/debugging
                try
                {
                    await _webhookLogRepo.AddAsync(new WebhookEventLog
                    {
                        EventType = eventType,
                        EntityId = null,
                        RawPayload = payload,
                        ReceivedAt = DateTime.UtcNow,
                        Processed = false,
                        ProcessingError = ex.Message
                    });
                }
                catch (Exception logEx)
                {
                    _logger.LogError(logEx, "Failed to write webhook error log.");
                }

                // Return 500 so Razorpay retries if there's a transient server issue
                return StatusCode(500);
            }
        }
    }
}
