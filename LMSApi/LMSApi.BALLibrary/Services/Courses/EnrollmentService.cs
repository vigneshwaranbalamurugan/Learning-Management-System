using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;

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

        public EnrollmentService(
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            ICourseBatchRepository batchRepository,
            IPaymentRepository paymentRepository,
            IMapper mapper,
            ILogger<EnrollmentService> logger)
        {
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _batchRepository = batchRepository;
            _mapper = mapper;
            _logger = logger;
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

            _logger.LogInformation("Student Enrolled in Free Course: UserId={UserId}, CourseId={CourseId}", userId, courseId);

            var saved = await _enrollmentRepository.GetActiveEnrollmentAsync(userId, courseId);
            return _mapper.Map<EnrollmentResponse>(saved);
        }

        public async Task<string> EnrollInPremiumCourseAsync(int userId, int courseId, int? batchId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
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

            // TODO: User to implement Razorpay Order Creation via Razorpay SDK here.
            // Example:
            // var order = razorpayClient.Order.Create(options);
            // var orderId = order["id"].ToString();
            string razorpayOrderId = "order_mock_" + Guid.NewGuid().ToString("N").Substring(0, 10);

            var payment = new Payments
            {
                UserId = userId,
                CourseId = courseId,
                Amount = course.Price ?? 0,
                RazorpayOrderId = razorpayOrderId,
                Status = PaymentStatus.Pending
            };

            await _paymentRepository.AddAsync(payment);

            _logger.LogInformation("Payment Created: OrderId={OrderId}, UserId={UserId}", razorpayOrderId, userId);

            return razorpayOrderId;
        }

        public async Task<EnrollmentResponse> VerifyPaymentAndEnrollAsync(int userId, int courseId, int? batchId, string razorpayOrderId, string razorpayPaymentId, string razorpaySignature)
        {
            var payment = await _paymentRepository.GetByRazorpayOrderIdAsync(razorpayOrderId);
            if (payment == null || payment.UserId != userId || payment.CourseId != courseId)
                throw new InvalidOperationException("Invalid payment record.");

            // TODO: User to implement Signature verification via Razorpay SDK here.
            // Example: Utils.verifyPaymentSignature(attributes);
            bool isSignatureValid = true; 

            if (!isSignatureValid)
            {
                payment.Status = PaymentStatus.Failed;
                await _paymentRepository.UpdateAsync(payment);
                throw new InvalidOperationException("Payment signature verification failed.");
            }

            payment.RazorpayPaymentId = razorpayPaymentId;
            payment.Status = PaymentStatus.Completed;
            payment.PaidAt = DateTime.UtcNow;
            payment.RawResponse = "Success"; // Optional raw response storage

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
                await EnrollSelfPacedAsync(enrollment, course, batchId);
            else
                await EnrollCohortBasedAsync(enrollment, course, batchId);

            // Link payment and enrollment
            enrollment.Payment = payment;

            await _enrollmentRepository.CreateEnrollmentAsync(enrollment);
            payment.EnrollmentId = enrollment.Id;
            await _paymentRepository.UpdateAsync(payment);

            _logger.LogInformation("Payment Verified and Enrolled: UserId={UserId}, CourseId={CourseId}", userId, courseId);

            var saved = await _enrollmentRepository.GetActiveEnrollmentAsync(userId, courseId);
            return _mapper.Map<EnrollmentResponse>(saved);
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
            if (course.DefaultAssignmentDeadlineDays.HasValue)
            {
                enrollment.AccessExpiresAt = enrollment.EnrolledAt
                    .AddDays(course.DefaultAssignmentDeadlineDays.Value);
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
