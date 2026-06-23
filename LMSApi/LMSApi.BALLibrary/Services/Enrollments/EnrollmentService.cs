using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;
using LMSApi.BALLibrary.Utils;

namespace LMSApi.BALLibrary.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseBatchRepository _batchRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<EnrollmentService> _logger;
        private readonly IPaymentService _paymentService;
        private readonly IPlatformFeeService _feeService;
        private readonly IInstructorPayoutService _payoutService;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly IUserNotificationsService _userNotificationsService;

        public EnrollmentService(
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            ICourseBatchRepository batchRepository,
            IPaymentRepository paymentRepository,
            IMapper mapper,
            ILogger<EnrollmentService> logger,
            IPaymentService paymentService,
            IPlatformFeeService feeService,
            IInstructorPayoutService payoutService,
            IUserRepository userRepository,
            INotificationService notificationService,
            IUserNotificationsService userNotificationsService)
        {
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _batchRepository = batchRepository;
            _paymentRepository = paymentRepository;
            _mapper = mapper;
            _logger = logger;
            _paymentService = paymentService;
            _feeService = feeService;
            _payoutService = payoutService;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _userNotificationsService = userNotificationsService;
        }

        public async Task<EnrollmentResponse> EnrollInFreeCourseAsync(int userId, int courseId, int? batchId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course.Status != CourseStatus.Published)
                throw new InvalidOperationException($"Course '{courseId}' is not published.");

            if (course.IsPremium)
                throw new InvalidOperationException("This is a premium course. Use EnrollInPremiumCourseAsync.");

            var existing = await _enrollmentRepository.GetActiveEnrollmentAsync(userId, courseId);
            if (existing != null)
                throw new InvalidOperationException($"User '{userId}' is already enrolled in course '{courseId}'.");

            await _enrollmentRepository.BeginTransactionAsync();
            try
            {
                var enrollment = new Enrollments
                {
                    UserId = userId,
                    CourseId = courseId,
                    EnrolledAt = DateTime.UtcNow,
                    EnrollmentStatus = EnrollmentStatus.Active,
                    ProgressPercentage = 0,
                    IsCompleted = false
                };

                if (course.CourseAccessType == CourseAccessType.SelfPaced)
                    await EnrollSelfPacedAsync(enrollment, course, batchId);
                else
                    await EnrollCohortBasedAsync(enrollment, course, batchId);

                await _enrollmentRepository.CreateEnrollmentAsync(enrollment);

                await _enrollmentRepository.CommitTransactionAsync();

                _logger.LogInformation("Student Enrolled in Free Course: UserId={UserId}, CourseId={CourseId}", userId, courseId);

                // ── Send Enrollment Email ──
                var learner = await _userRepository.GetByIdAsync(userId);
                var learnerName = learner.UserProfile?.FirstName ?? learner.Email;
                string? batchName = null;
                if (batchId.HasValue)
                {
                    var batch = await _batchRepository.GetByIdAsync(batchId.Value);
                    batchName = batch.Name;
                }
                var html = EmailTemplate.GetCourseEnrollmentTemplate(learnerName, course.Title, course.CourseAccessType, batchName);
                Message msg = new EmailMessage(learner.Email, $"You're enrolled in {course.Title}!", html) { IsHtml = true };
                await _notificationService.Send(msg);

                try
                {
                    await _userNotificationsService.CreateAndSendNotificationAsync(
                        userId: userId,
                        title: "Course Enrollment",
                        message: $"You have successfully enrolled in the course: {course.Title}",
                        type: NotificationType.CourseEnrollment,
                        redirectUrl: $"/course/{course.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send course enrollment realtime notification for User {UserId}", userId);
                }

                var saved = await _enrollmentRepository.GetActiveEnrollmentAsync(userId, courseId);
                return _mapper.Map<EnrollmentResponse>(saved);
            }
            catch
            {
                await _enrollmentRepository.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<string> EnrollInPremiumCourseAsync(int userId, int courseId, int? batchId, string providerName)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);

            if (course.Status != CourseStatus.Published)
                throw new InvalidOperationException($"Course '{courseId}' is not published.");

            if (!course.IsPremium)
                throw new InvalidOperationException("Course is free. Use EnrollInFreeCourseAsync.");

            var existing = await _enrollmentRepository.GetActiveEnrollmentAsync(userId, courseId);
            if (existing != null)
                throw new InvalidOperationException("Already enrolled.");

            if (course.CourseAccessType != CourseAccessType.SelfPaced)
            {
                if (!batchId.HasValue || !await _enrollmentRepository.ValidateBatchEnrollmentAsync(batchId.Value))
                    throw new InvalidOperationException("Invalid or full batch.");
            }

            // Check for existing pending/failed payments to avoid duplicate Razorpay orders
            var existingPayments = await _paymentRepository.GetPaymentsByUserAsync(userId);
            var pendingOrFailedPayment = existingPayments.FirstOrDefault(p => p.CourseId == courseId && (p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Failed));

            if (pendingOrFailedPayment != null)
            {
                if (pendingOrFailedPayment.CreatedAt >= DateTime.UtcNow.AddMinutes(-30))
                {
                    _logger.LogInformation("Returning existing pending/failed payment: OrderId={OrderId}", pendingOrFailedPayment.ProviderOrderId);

                    // Reset Failed status to Pending for new attempt
                    if (pendingOrFailedPayment.Status == PaymentStatus.Failed)
                    {
                        pendingOrFailedPayment.Status = PaymentStatus.Pending;
                        await _paymentRepository.UpdateAsync(pendingOrFailedPayment);
                    }

                    return pendingOrFailedPayment.ProviderOrderId;
                }
                else if (pendingOrFailedPayment.Status == PaymentStatus.Pending)
                {
                    // Expire the old pending payment record so it doesn't conflict
                    pendingOrFailedPayment.Status = PaymentStatus.Failed;
                    await _paymentRepository.UpdateAsync(pendingOrFailedPayment);
                }
            }

            // ── Calculate platform fee split ───────────────────────────────────
            var totalAmount = course.Price ?? 0;
            var (platformFeeAmount, instructorAmount, feeConfig) =
                await _feeService.CalculateSplitAsync(totalAmount, FeeCategory.CourseFee);

            string receiptId = $"rcpt_{userId}_{courseId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            string providerOrderId = await _paymentService.CreateOrderAsync(providerName, totalAmount, "INR", receiptId);

            var payment = new Payments
            {
                UserId = userId,
                CourseId = courseId,
                BatchId = batchId,
                Amount = totalAmount,
                ProviderOrderId = providerOrderId,
                Status = PaymentStatus.Pending,
                // ── Store fee snapshot at order creation time ────────────────
                PlatformFeeConfigId = feeConfig?.Id,
                PlatformFeeAmount = platformFeeAmount,
                InstructorAmount = instructorAmount,
                FeeTypeSnapshot = feeConfig?.FeeType,
                FeeValueSnapshot = feeConfig?.Value
            };

            await _paymentRepository.AddAsync(payment);

            _logger.LogInformation(
                "Payment Created: OrderId={OrderId}, UserId={UserId}, TotalAmount={Total}, PlatformFee={Fee}, InstructorAmount={Instructor}",
                providerOrderId, userId, totalAmount, platformFeeAmount, instructorAmount);

            return providerOrderId;
        }

        public async Task<EnrollmentResponse> VerifyPaymentAndEnrollAsync(int userId, int courseId, VerifyPaymentRequest request)
        {
            var payment = await _paymentRepository.GetByProviderOrderIdAsync(request.ProviderOrderId);
            if (payment == null || payment.UserId != userId || payment.CourseId != courseId)
                throw new InvalidOperationException("Invalid payment record.");

            if (payment.Status == PaymentStatus.Completed)
            {
                _logger.LogInformation("Payment {OrderId} already completed via webhook. Returning existing enrollment.", request.ProviderOrderId);
                var existing = await _enrollmentRepository.GetActiveEnrollmentAsync(userId, courseId);
                return _mapper.Map<EnrollmentResponse>(existing);
            }

            bool isSignatureValid = _paymentService.VerifySignature(request.ProviderName, request.ProviderOrderId, request.ProviderPaymentId, request.ProviderSignature);

            if (!isSignatureValid)
            {
                payment.Status = PaymentStatus.Failed;
                await _paymentRepository.UpdateAsync(payment);
                throw new InvalidOperationException("Payment signature verification failed.");
            }

            await _enrollmentRepository.BeginTransactionAsync();
            try
            {
                payment.ProviderPaymentId = request.ProviderPaymentId;
                payment.Status = PaymentStatus.Completed;
                payment.PaidAt = DateTime.UtcNow;
                payment.RawResponse = "Success";

                var course = await _courseRepository.GetByIdAsync(courseId);
                var enrollment = new Enrollments
                {
                    UserId = userId,
                    CourseId = courseId,
                    EnrolledAt = DateTime.UtcNow,
                    EnrollmentStatus = EnrollmentStatus.Active,
                    ProgressPercentage = 0,
                    IsCompleted = false
                };

                if (course.CourseAccessType == CourseAccessType.SelfPaced)
                    await EnrollSelfPacedAsync(enrollment, course, request.BatchId);
                else
                    await EnrollCohortBasedAsync(enrollment, course, request.BatchId);

                // Link payment and enrollment
                enrollment.Payment = payment;

                await _enrollmentRepository.CreateEnrollmentAsync(enrollment);
                payment.EnrollmentId = enrollment.Id;
                await _paymentRepository.UpdateAsync(payment);

                await _enrollmentRepository.CommitTransactionAsync();

                _logger.LogInformation("Payment Verified and Enrolled: UserId={UserId}, CourseId={CourseId}", userId, courseId);

                // ── Send Enrollment Email ──
                var learner = await _userRepository.GetByIdAsync(userId);
                var learnerName = learner.UserProfile?.FirstName ?? learner.Email;
                string? batchName = null;
                if (request.BatchId.HasValue)
                {
                    var batch = await _batchRepository.GetByIdAsync(request.BatchId.Value);
                    batchName = batch.Name;
                }
                var html = EmailTemplate.GetCourseEnrollmentTemplate(learnerName, course.Title, course.CourseAccessType, batchName);
                Message msg = new EmailMessage(learner.Email, $"You're enrolled in {course.Title}!", html) { IsHtml = true };
                await _notificationService.Send(msg);

                try
                {
                    await _userNotificationsService.CreateAndSendNotificationAsync(
                        userId: userId,
                        title: "Course Enrollment",
                        message: $"You have successfully enrolled in the course: {course.Title}",
                        type: NotificationType.CourseEnrollment,
                        redirectUrl: $"/course/{course.Id}");

                    await _userNotificationsService.CreateAndSendNotificationAsync(
                        userId: userId,
                        title: "Payment Successful",
                        message: $"Payment of {payment.Amount} for '{course.Title}' was successful.",
                        type: NotificationType.PaymentSuccess);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send enrollment/payment success realtime notification for User {UserId}", userId);
                }

                // ── Initiate instructor payout ───
                if (payment.InstructorAmount > 0)
                {
                    try
                    {
                        await _payoutService.InitiatePayoutAsync(payment, course.InstructorId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Instructor payout initiation failed for PaymentId={PaymentId}, InstructorId={InstructorId}",
                            payment.Id, course.InstructorId);
                    }
                }

                var saved = await _enrollmentRepository.GetActiveEnrollmentAsync(userId, courseId);
                return _mapper.Map<EnrollmentResponse>(saved);
            }
            catch
            {
                await _enrollmentRepository.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task ProcessWebhookPaymentAsync(string providerOrderId, string providerPaymentId, string eventType, string? rawResponse = null)
        {
            var payment = await _paymentRepository.GetByProviderOrderIdAsync(providerOrderId);
            if (payment == null)
            {
                _logger.LogWarning("Webhook received for unknown OrderId: {OrderId}", providerOrderId);
                return;
            }

            if (payment.Status == PaymentStatus.Completed)
            {
                _logger.LogInformation("Webhook ignored: Payment {OrderId} is already Completed.", providerOrderId);
                return;
            }

            if (eventType == "order.paid" || eventType == "payment.captured")
            {
                await _enrollmentRepository.BeginTransactionAsync();
                try
                {
                    payment.ProviderPaymentId = providerPaymentId;
                    payment.Status = PaymentStatus.Completed;
                    payment.PaidAt = DateTime.UtcNow;
                    payment.RawResponse = rawResponse ?? "WebhookSuccess";

                    var course = await _courseRepository.GetByIdAsync(payment.CourseId);
                    var enrollment = new Enrollments
                    {
                        UserId = payment.UserId,
                        CourseId = payment.CourseId,
                        EnrolledAt = DateTime.UtcNow,
                        EnrollmentStatus = EnrollmentStatus.Active,
                        ProgressPercentage = 0,
                        IsCompleted = false
                    };

                    if (course.CourseAccessType == CourseAccessType.SelfPaced)
                        await EnrollSelfPacedAsync(enrollment, course, payment.BatchId);
                    else
                        await EnrollCohortBasedAsync(enrollment, course, payment.BatchId);

                    enrollment.Payment = payment;

                    await _enrollmentRepository.CreateEnrollmentAsync(enrollment);
                    payment.EnrollmentId = enrollment.Id;
                    await _paymentRepository.UpdateAsync(payment);

                    await _enrollmentRepository.CommitTransactionAsync();

                    _logger.LogInformation("Webhook Payment Verified and Enrolled: UserId={UserId}, CourseId={CourseId}", payment.UserId, payment.CourseId);

                    // ── Send Enrollment Email ──
                    var learner = await _userRepository.GetByIdAsync(payment.UserId);
                    var learnerName = learner.UserProfile?.FirstName ?? learner.Email;
                    string? batchName = null;
                    if (payment.BatchId.HasValue)
                    {
                        var batch = await _batchRepository.GetByIdAsync(payment.BatchId.Value);
                        batchName = batch.Name;
                    }
                    var html = EmailTemplate.GetCourseEnrollmentTemplate(learnerName, course.Title, course.CourseAccessType, batchName);
                    Message msg = new EmailMessage(learner.Email, $"You're enrolled in {course.Title}!", html) { IsHtml = true };
                    await _notificationService.Send(msg);

                    try
                    {
                        await _userNotificationsService.CreateAndSendNotificationAsync(
                            userId: payment.UserId,
                            title: "Course Enrollment",
                            message: $"You have successfully enrolled in the course: {course.Title}",
                            type: NotificationType.CourseEnrollment,
                            redirectUrl: $"/course/{course.Id}");

                        await _userNotificationsService.CreateAndSendNotificationAsync(
                            userId: payment.UserId,
                            title: "Payment Successful",
                            message: $"Payment of {payment.Amount} for '{course.Title}' was successful.",
                            type: NotificationType.PaymentSuccess);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send enrollment/payment success realtime notification for User {UserId}", payment.UserId);
                    }

                    if (payment.InstructorAmount > 0)
                    {
                        try
                        {
                            await _payoutService.InitiatePayoutAsync(payment, course.InstructorId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Instructor payout initiation failed from webhook for PaymentId={PaymentId}", payment.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await _enrollmentRepository.RollbackTransactionAsync();
                    _logger.LogError(ex, "Failed to process webhook completion for OrderId: {OrderId}", providerOrderId);
                    throw;
                }
            }
            else if (eventType == "payment.failed")
            {
                payment.ProviderPaymentId = providerPaymentId;
                payment.Status = PaymentStatus.Failed;
                payment.RawResponse = rawResponse ?? "WebhookFailed";
                await _paymentRepository.UpdateAsync(payment);
                _logger.LogInformation("Webhook Payment Failed: OrderId={OrderId}, PaymentId={PaymentId}", providerOrderId, providerPaymentId);

                try
                {
                    var course = await _courseRepository.GetByIdAsync(payment.CourseId);
                    await _userNotificationsService.CreateAndSendNotificationAsync(
                        userId: payment.UserId,
                        title: "Payment Failed",
                        message: $"Payment of {payment.Amount} for '{course.Title}' failed.",
                        type: NotificationType.PaymentFailed);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send payment failed realtime notification for User {UserId}", payment.UserId);
                }
            }
            else if (eventType == "payment.authorized" || eventType == "payment.pending")
            {
                payment.ProviderPaymentId = providerPaymentId;
                payment.Status = PaymentStatus.Pending;
                payment.RawResponse = rawResponse ?? $"WebhookEvent:{eventType}";
                await _paymentRepository.UpdateAsync(payment);
                _logger.LogInformation("Webhook Payment Pending/Attempted: OrderId={OrderId}, PaymentId={PaymentId}", providerOrderId, providerPaymentId);
            }
            else
            {
                payment.ProviderPaymentId = providerPaymentId;
                payment.RawResponse = rawResponse ?? $"WebhookEvent:{eventType}";
                await _paymentRepository.UpdateAsync(payment);
                _logger.LogInformation("Webhook event {EventType} recorded for OrderId: {OrderId}", eventType, providerOrderId);
            }
        }

        public async Task<DateTime?> CalculateAssignmentDeadlineAsync(int userId, int assignmentId)
        {
            // Will call Postgres function `calculate_assignment_deadline` via context or raw ADO.NET
            // Mocking for now to use the same pattern as GetAvailableSeats
            // The EF context doesn't natively expose this, so assuming manual or standard EF raw query.
            return null; // Update with Postgres execution if needed
        }

        public async Task<bool> ValidateCourseAccessAsync(int enrollmentId)
        {
            var accessExpiresAt = await _enrollmentRepository.GetCourseAccessAsync(enrollmentId);
            if (accessExpiresAt.HasValue && accessExpiresAt.Value < DateTime.UtcNow)
                return false;
            return true;
        }

        public async Task<bool> ValidateBatchEnrollmentAsync(int batchId)
        {
            return await _enrollmentRepository.ValidateBatchEnrollmentAsync(batchId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EnrollmentResponse>> GetMyEnrollmentsAsync(int userId)
        {
            var enrollments = await _enrollmentRepository.GetEnrollmentsByUserAsync(userId);
            return _mapper.Map<IEnumerable<EnrollmentResponse>>(enrollments);
        }

        // ─── Private helpers ──────────────────────────────────────────────────

        private static Task EnrollSelfPacedAsync(
            Enrollments enrollment,
            Courses course,
            int? batchId)
        {
            // SelfPaced must not have a batch
            if (batchId.HasValue)
                throw new InvalidOperationException(
                    "SelfPaced courses do not use batches. Remove the BatchId from the request.");

            enrollment.BatchId = null;

            // Auto-calculate AccessExpiresAt if the course has a deadline configured
            if (course.DefaultDeadlineDays.HasValue)
            {
                enrollment.AccessExpiresAt = enrollment.EnrolledAt
                    .AddDays(course.DefaultDeadlineDays.Value);
            }
            else
            {
                enrollment.AccessExpiresAt = null; // never expires
            }

            return Task.CompletedTask;
        }

        private async Task EnrollCohortBasedAsync(
            Enrollments enrollment,
            Courses course,
            int? batchId)
        {
            // CohortBased requires a batch
            if (!batchId.HasValue)
                throw new InvalidOperationException(
                    "CohortBased courses require a BatchId to enroll.");

            var batch = await _batchRepository.GetByIdAsync(batchId.Value);

            // Batch must belong to the requested course
            if (batch.CourseId != course.Id)
                throw new InvalidOperationException(
                    $"Batch '{batchId}' does not belong to course '{course.Id}'.");

            // Batch must be in the enrollment window
            var now = DateTime.UtcNow;
            if (now < batch.EnrollmentStartDate || now > batch.EnrollmentEndDate)
                throw new InvalidOperationException(
                    $"Enrollment window for batch '{batchId}' is not currently open.");

            // Seat availability check via PostgreSQL function
            var availableSeats = await _batchRepository.GetAvailableSeatsAsync(batchId.Value);
            if (availableSeats <= 0)
                throw new InvalidOperationException(
                    $"Batch '{batchId}' is full. No seats available.");

            enrollment.BatchId = batchId.Value;
            // Access expires when the batch ends
            enrollment.AccessExpiresAt = batch.EndDate;

            _logger.LogInformation("Student Enrolled In Batch: UserId={UserId}, BatchId={BatchId}, CourseId={CourseId}",
                enrollment.UserId, batchId, course.Id);
        }
    }
}
