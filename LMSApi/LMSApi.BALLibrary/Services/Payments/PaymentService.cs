using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;

namespace LMSApi.BALLibrary.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IEnumerable<IPaymentProvider> _providers;
        private readonly IPaymentRepository _paymentRepo;
        private readonly IPlatformFeeService _feeService;
        private readonly ICourseRepository _courseRepo;
        private readonly ILogger<PaymentService>? _logger;

        public PaymentService(
            IEnumerable<IPaymentProvider> providers,
            IPaymentRepository paymentRepo,
            IPlatformFeeService feeService,
            ICourseRepository courseRepo,
            ILogger<PaymentService>? logger = null)
        {
            _providers = providers;
            _paymentRepo = paymentRepo;
            _feeService = feeService;
            _courseRepo = courseRepo;
            _logger = logger;
        }

        public Task<string> CreateOrderAsync(string providerName, decimal amount, string currency, string receiptId)
        {
            var provider = GetProvider(providerName);
            return provider.CreateOrderAsync(amount, currency, receiptId);
        }

        public bool VerifySignature(string providerName, string orderId, string paymentId, string signature)
        {
            var provider = GetProvider(providerName);
            return provider.VerifySignature(orderId, paymentId, signature);
        }

        public async Task<(string orderId, decimal platformFee, decimal instructorAmount, int? configId)>
            CreateCourseOrderAsync(int courseId, string currency = "INR")
        {
            _logger?.LogInformation("Creating course order for Course ID: {CourseId}, Currency: {Currency}", courseId, currency);

            // 1. Fetch Course Price
            var course = await _courseRepo.GetByIdAsync(courseId)
                ?? throw new KeyNotFoundException($"Course {courseId} not found.");

            if (!course.Price.HasValue || course.Price.Value <= 0)
            {
                _logger?.LogWarning("Failed to create course order: Course ID: {CourseId} is free or price not set.", courseId);
                throw new InvalidOperationException("Cannot create an order for a free course.");
            }

            decimal totalAmount = course.Price.Value;

            // 2. Calculate the split (Platform Fee vs Instructor Amount)
            var splitResult = await _feeService.CalculateSplitAsync(totalAmount, FeeCategory.CourseFee, DateTime.UtcNow);
            _logger?.LogInformation("Calculated split for Course ID {CourseId}: Total: {TotalAmount}, Platform Fee: {PlatformFee}, Instructor Amount: {InstructorAmount}",
                courseId, totalAmount, splitResult.platformFeeAmount, splitResult.instructorAmount);

            // 3. Create Order via Provider (Razorpay)
            // The order is created for the FULL amount to charge the student.
            string receiptId = $"rcpt_course_{courseId}_{Guid.NewGuid():N}";
            var provider = GetProvider("Razorpay");
            string orderId = await provider.CreateOrderAsync(totalAmount, currency, receiptId);
            _logger?.LogInformation("Course order created successfully: Order ID: {OrderId}, Receipt ID: {ReceiptId}", orderId, receiptId);

            return (
                orderId, 
                splitResult.platformFeeAmount, 
                splitResult.instructorAmount, 
                splitResult.configUsed?.Id
            );
        }

        public async Task<Payments> ConfirmPaymentAsync(
            string providerOrderId,
            string providerPaymentId,
            string signature,
            int userId,
            int courseId,
            int? enrollmentId,
            int? platformFeeConfigId,
            decimal platformFeeAmount,
            decimal instructorAmount,
            FeeType? feeTypeSnapshot,
            decimal? feeValueSnapshot)
        {
            _logger?.LogInformation("Confirming payment for Provider Order ID: {OrderId}, Payment ID: {PaymentId}", providerOrderId, providerPaymentId);

            // Verify Razorpay signature
            var provider = GetProvider("Razorpay");
            if (!provider.VerifySignature(providerOrderId, providerPaymentId, signature))
            {
                _logger?.LogWarning("Payment confirmation failed: Signature verification failed for Order ID: {OrderId}", providerOrderId);
                throw new UnauthorizedAccessException("Payment signature verification failed.");
            }

            // Fetch existing pending payment record
            var payment = await _paymentRepo.GetByProviderOrderIdAsync(providerOrderId)
                ?? throw new KeyNotFoundException($"No payment record found for order {providerOrderId}");

            // Update with confirmation details
            payment.ProviderPaymentId = providerPaymentId;
            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;
            payment.EnrollmentId = enrollmentId;
            payment.PlatformFeeConfigId = platformFeeConfigId;
            payment.PlatformFeeAmount = platformFeeAmount;
            payment.InstructorAmount = instructorAmount;
            payment.FeeTypeSnapshot = feeTypeSnapshot;
            payment.FeeValueSnapshot = feeValueSnapshot;

            await _paymentRepo.UpdateAsync(payment);
            _logger?.LogInformation("Payment confirmed and updated in database. Payment ID: {PaymentId}, Status: Completed", payment.Id);
            return payment;
        }

        private IPaymentProvider GetProvider(string providerName)
        {
            var provider = _providers.FirstOrDefault(p =>
                p.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));
            return provider ?? throw new NotSupportedException(
                $"Payment provider '{providerName}' is not supported.");
        }
    }
}
