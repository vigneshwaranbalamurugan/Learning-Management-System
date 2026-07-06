using System;
using System.Linq;
using System.Threading.Tasks;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Services;
using LMSApi.DALLibrary.Repositories;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LMSApi.Tests.Services
{
    [TestFixture]
    public class EnrollmentServiceTests : BaseServiceTest
    {
        private Mock<ILogger<EnrollmentService>> _mockLogger = null!;
        private Mock<IPaymentService> _mockPaymentService = null!;
        private Mock<IPlatformFeeService> _mockPlatformFeeService = null!;
        private Mock<IInstructorPayoutService> _mockInstructorPayoutService = null!;
        private Mock<INotificationService> _mockNotificationService = null!;
        private Mock<IUserNotificationsService> _mockUserNotificationsService = null!;
        private Mock<IInvoiceService> _mockInvoiceService = null!;
        private IEnrollmentService _enrollmentService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mockLogger = new Mock<ILogger<EnrollmentService>>();
            _mockPaymentService = new Mock<IPaymentService>();
            _mockPlatformFeeService = new Mock<IPlatformFeeService>();
            _mockInstructorPayoutService = new Mock<IInstructorPayoutService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockUserNotificationsService = new Mock<IUserNotificationsService>();
            _mockInvoiceService = new Mock<IInvoiceService>();
            
            var enrollmentRepository = new EnrollmentRepository(DbContext);
            var courseRepository = new CourseRepository(DbContext);
            var batchRepository = new CourseBatchRepository(DbContext);
            var paymentRepository = new PaymentRepository(DbContext);
            var userRepository = new UserRepository(DbContext);

            _enrollmentService = new EnrollmentService(
                enrollmentRepository,
                courseRepository,
                batchRepository,
                paymentRepository,
                Mapper,
                _mockLogger.Object,
                _mockPaymentService.Object,
                _mockPlatformFeeService.Object,
                _mockInstructorPayoutService.Object,
                userRepository,
                _mockNotificationService.Object,
                _mockUserNotificationsService.Object,
                _mockInvoiceService.Object
            );
        }

        private async Task<(Users student, Courses course)> SetupCourse(
            CourseAccessType type,
            bool isPremium = false,
            CourseStatus status = CourseStatus.Published)
        {
            var student = new Users { Email = $"student-{Guid.NewGuid()}@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Learner", Description = "Desc" } };
            var cat = new CourseCategories { Name = "Cat", Description = "Desc" };
            var inst = new Users { Email = $"inst-{Guid.NewGuid()}@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Instructor", Description = "Desc" } };
            DbContext.Users.Add(student);
            DbContext.Users.Add(inst);
            DbContext.CourseCategories.Add(cat);
            await DbContext.SaveChangesAsync();

            var course = new Courses
            {
                Title = "Course", Description = "Desc",
                Price = isPremium ? 100m : 0m,
                ThumbnailUrl = "url", IntroVideoUrl = "url",
                IsPremium = isPremium,
                Requirements = "Reqs", LearningOutcomes = "Outcomes",
                EstimatedDuration = TimeSpan.Zero,
                Level = CourseLevel.Beginner, LanguageId = 1,
                PublishedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
                DefaultDeadlineDays = 7, CategoryId = cat.Id, InstructorId = inst.Id,
                slug = Guid.NewGuid().ToString(), Status = status, CourseAccessType = type,
            };
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            return (student, course);
        }

        // ─── EnrollInFreeCourseAsync ───────────────────────────────────────────

        [Test]
        public async Task EnrollInFreeCourseAsync_SelfPaced_CreatesEnrollment()
        {
            var (student, course) = await SetupCourse(CourseAccessType.SelfPaced);
            
            var result = await _enrollmentService.EnrollInFreeCourseAsync(student.Id, course.Id, null);

            Assert.That(result.CourseId, Is.EqualTo(course.Id));
            Assert.That(result.UserId, Is.EqualTo(student.Id));
            Assert.That(result.BatchId, Is.Null);

            await Task.Delay(100);
            _mockNotificationService.Verify(x => x.Send(It.IsAny<Message>()), Times.Once);
        }

        [Test]
        public async Task EnrollInFreeCourseAsync_PremiumCourse_ThrowsInvalidOperationException()
        {
            var (student, course) = await SetupCourse(CourseAccessType.SelfPaced, isPremium: true);
            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _enrollmentService.EnrollInFreeCourseAsync(student.Id, course.Id, null));
        }

        [Test]
        public async Task EnrollInFreeCourseAsync_CourseNotPublished_ThrowsInvalidOperationException()
        {
            var (student, course) = await SetupCourse(CourseAccessType.SelfPaced, status: CourseStatus.Draft);
            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _enrollmentService.EnrollInFreeCourseAsync(student.Id, course.Id, null));
        }

        [Test]
        public async Task EnrollInFreeCourseAsync_AlreadyEnrolled_ThrowsInvalidOperationException()
        {
            var (student, course) = await SetupCourse(CourseAccessType.SelfPaced);
            DbContext.Enrollments.Add(new Enrollments
            {
                CourseId = course.Id, UserId = student.Id,
                EnrollmentStatus = EnrollmentStatus.Active,
                EnrolledAt = DateTime.UtcNow, ProgressPercentage = 0, IsCompleted = false
            });
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _enrollmentService.EnrollInFreeCourseAsync(student.Id, course.Id, null));
        }

        [Test]
        public async Task EnrollInFreeCourseAsync_CohortBasedWithoutBatchId_ThrowsException()
        {
            var (student, course) = await SetupCourse(CourseAccessType.CohortBased);
            
            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _enrollmentService.EnrollInFreeCourseAsync(student.Id, course.Id, null));
        }

        [Test]
        public async Task EnrollInFreeCourseAsync_SelfPacedWithBatchId_ThrowsInvalidOperationException()
        {
            var (student, course) = await SetupCourse(CourseAccessType.SelfPaced);
            // Providing a batchId for a SelfPaced course is forbidden
            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _enrollmentService.EnrollInFreeCourseAsync(student.Id, course.Id, batchId: 1));
        }

        // ─── ValidateCourseAccessAsync ─────────────────────────────────────────

        [Test]
        public async Task ValidateCourseAccessAsync_ActiveEnrollment_ReturnsTrue()
        {
            var (student, course) = await SetupCourse(CourseAccessType.SelfPaced);
            var enrollment = new Enrollments
            {
                CourseId = course.Id, UserId = student.Id,
                EnrollmentStatus = EnrollmentStatus.Active,
                EnrolledAt = DateTime.UtcNow, ProgressPercentage = 0, IsCompleted = false
            };
            DbContext.Enrollments.Add(enrollment);
            await DbContext.SaveChangesAsync();

            var hasAccess = await _enrollmentService.ValidateCourseAccessAsync(enrollment.Id);

            Assert.That(hasAccess, Is.True);
        }

        [Test]
        public async Task ValidateCourseAccessAsync_ExpiredAccess_ReturnsFalse()
        {
            var (student, course) = await SetupCourse(CourseAccessType.SelfPaced);
            var enrollment = new Enrollments
            {
                CourseId = course.Id, UserId = student.Id,
                EnrollmentStatus = EnrollmentStatus.Active,
                EnrolledAt = DateTime.UtcNow.AddDays(-30),
                AccessExpiresAt = DateTime.UtcNow.AddDays(-1), // expired
                ProgressPercentage = 0, IsCompleted = false
            };
            DbContext.Enrollments.Add(enrollment);
            await DbContext.SaveChangesAsync();

            var hasAccess = await _enrollmentService.ValidateCourseAccessAsync(enrollment.Id);

            Assert.That(hasAccess, Is.False);
        }

        // ─── GetMyEnrollmentsAsync ─────────────────────────────────────────────

        [Test]
        public async Task GetMyEnrollmentsAsync_NoEnrollments_ReturnsEmpty()
        {
            var result = await _enrollmentService.GetMyEnrollmentsAsync(99999);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetMyEnrollmentsAsync_WithEnrollments_ReturnsAll()
        {
            var (student, course) = await SetupCourse(CourseAccessType.SelfPaced);
            DbContext.Enrollments.Add(new Enrollments
            {
                CourseId = course.Id, UserId = student.Id,
                EnrollmentStatus = EnrollmentStatus.Active,
                EnrolledAt = DateTime.UtcNow, ProgressPercentage = 0, IsCompleted = false
            });
            await DbContext.SaveChangesAsync();

            var result = await _enrollmentService.GetMyEnrollmentsAsync(student.Id);
            Assert.That(result.Count(), Is.EqualTo(1));
        }

        // ─── EnrollInPremiumCourseAsync ───────────────────────────────────────
        [Test]
        public async Task EnrollInPremiumCourseAsync_CreatesPendingPaymentAndReturnsOrderId()
        {
            var (student, course) = await SetupCourse(CourseAccessType.SelfPaced, isPremium: true);
            var expectedOrderId = "order_test_123";

            _mockPlatformFeeService.Setup(f => f.CalculateSplitAsync(course.Price ?? 0m, FeeCategory.CourseFee, It.IsAny<System.DateTime>()))
                .ReturnsAsync((10m, 90m, new PlatformFeeConfig { Id = 1 }));

            _mockPaymentService.Setup(p => p.CreateOrderAsync("Razorpay", course.Price ?? 0m, "INR", It.IsAny<string>()))
                .ReturnsAsync(expectedOrderId);

            var result = await _enrollmentService.EnrollInPremiumCourseAsync(student.Id, course.Id, null, "Razorpay");

            Assert.That(result, Is.EqualTo(expectedOrderId));
            var payment = DbContext.Payments.FirstOrDefault(p => p.ProviderOrderId == expectedOrderId);
            Assert.That(payment, Is.Not.Null);
            Assert.That(payment!.Status, Is.EqualTo(PaymentStatus.Pending));
        }

        [Test]
        public async Task EnrollInPremiumCourseAsync_ReusesUnexpiredPendingPayment()
        {
            var (student, course) = await SetupCourse(CourseAccessType.SelfPaced, isPremium: true);
            var existingOrderId = "order_existing";
            var existingPayment = new Payments
            {
                UserId = student.Id,
                CourseId = course.Id,
                Status = PaymentStatus.Pending,
                ProviderOrderId = existingOrderId,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                Amount = 100
            };
            DbContext.Payments.Add(existingPayment);
            await DbContext.SaveChangesAsync();

            var result = await _enrollmentService.EnrollInPremiumCourseAsync(student.Id, course.Id, null, "Razorpay");

            Assert.That(result, Is.EqualTo(existingOrderId));
            _mockPaymentService.Verify(p => p.CreateOrderAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task EnrollInPremiumCourseAsync_ReusesUnexpiredFailedPayment()
        {
            var (student, course) = await SetupCourse(CourseAccessType.SelfPaced, isPremium: true);
            var existingOrderId = "order_existing_failed";
            var existingPayment = new Payments
            {
                UserId = student.Id,
                CourseId = course.Id,
                Status = PaymentStatus.Failed,
                ProviderOrderId = existingOrderId,
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                Amount = 100
            };
            DbContext.Payments.Add(existingPayment);
            await DbContext.SaveChangesAsync();

            var result = await _enrollmentService.EnrollInPremiumCourseAsync(student.Id, course.Id, null, "Razorpay");

            Assert.That(result, Is.EqualTo(existingOrderId));
            var updatedPayment = DbContext.Payments.FirstOrDefault(p => p.ProviderOrderId == existingOrderId);
            Assert.That(updatedPayment!.Status, Is.EqualTo(PaymentStatus.Pending));
            _mockPaymentService.Verify(p => p.CreateOrderAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        // ─── VerifyPaymentAndEnrollAsync ──────────────────────────────────────

        [Test]
        public async Task VerifyPaymentAndEnrollAsync_AlreadyCompleted_ReturnsExistingEnrollment_Idempotent()
        {
            var (student, course) = await SetupCourse(CourseAccessType.SelfPaced, isPremium: true);
            var orderId = "order_completed";
            
            var payment = new Payments
            {
                UserId = student.Id,
                CourseId = course.Id,
                Status = PaymentStatus.Completed, // Already completed
                ProviderOrderId = orderId,
                Amount = 100
            };
            DbContext.Payments.Add(payment);

            var enrollment = new Enrollments
            {
                CourseId = course.Id,
                UserId = student.Id,
                EnrollmentStatus = EnrollmentStatus.Active,
                EnrolledAt = DateTime.UtcNow,
                IsCompleted = false
            };
            DbContext.Enrollments.Add(enrollment);
            await DbContext.SaveChangesAsync();

            var verifyRequest = new VerifyPaymentRequest
            {
                ProviderName = "Razorpay",
                ProviderOrderId = orderId,
                ProviderPaymentId = "pay_123",
                ProviderSignature = "sig_123",
                BatchId = null
            };
            var result = await _enrollmentService.VerifyPaymentAndEnrollAsync(student.Id, course.Id, verifyRequest);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.CourseId, Is.EqualTo(course.Id));
            
            // Should not verify signature if already completed
            _mockPaymentService.Verify(p => p.VerifySignature(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        // ─── ProcessWebhookPaymentAsync ───────────────────────────────────────

        [Test]
        public async Task ProcessWebhookPaymentAsync_MarksPaymentCompleted_AndEnrollsStudent()
        {
            var (student, course) = await SetupCourse(CourseAccessType.SelfPaced, isPremium: true);
            var orderId = "order_webhook";
            var payId = "pay_webhook";
            
            var payment = new Payments
            {
                UserId = student.Id,
                CourseId = course.Id,
                Status = PaymentStatus.Pending,
                ProviderOrderId = orderId,
                Amount = 100
            };
            DbContext.Payments.Add(payment);
            await DbContext.SaveChangesAsync();

            await _enrollmentService.ProcessWebhookPaymentAsync(orderId, payId, "order.paid");

            var updatedPayment = DbContext.Payments.FirstOrDefault(p => p.ProviderOrderId == orderId);
            Assert.That(updatedPayment!.Status, Is.EqualTo(PaymentStatus.Completed));
            Assert.That(updatedPayment.ProviderPaymentId, Is.EqualTo(payId));

            var enrollment = DbContext.Enrollments.FirstOrDefault(e => e.UserId == student.Id && e.CourseId == course.Id);
            Assert.That(enrollment, Is.Not.Null);
            Assert.That(enrollment!.EnrollmentStatus, Is.EqualTo(EnrollmentStatus.Active));

            await Task.Delay(100);
            _mockNotificationService.Verify(x => x.Send(It.IsAny<Message>()), Times.Exactly(2));
        }

        [Test]
        public async Task ProcessWebhookPaymentAsync_FailedEvent_MarksPaymentFailed()
        {
            var (student, course) = await SetupCourse(CourseAccessType.SelfPaced, isPremium: true);
            var orderId = "order_webhook_failed";
            var payId = "pay_webhook_failed";
            
            var payment = new Payments
            {
                UserId = student.Id,
                CourseId = course.Id,
                Status = PaymentStatus.Pending,
                ProviderOrderId = orderId,
                Amount = 100
            };
            DbContext.Payments.Add(payment);
            await DbContext.SaveChangesAsync();

            await _enrollmentService.ProcessWebhookPaymentAsync(orderId, payId, "payment.failed", "{\"error\": \"failed\"}");

            var updatedPayment = DbContext.Payments.FirstOrDefault(p => p.ProviderOrderId == orderId);
            Assert.That(updatedPayment!.Status, Is.EqualTo(PaymentStatus.Failed));
            Assert.That(updatedPayment.ProviderPaymentId, Is.EqualTo(payId));
            Assert.That(updatedPayment.RawResponse, Is.EqualTo("{\"error\": \"failed\"}"));

            var enrollment = DbContext.Enrollments.FirstOrDefault(e => e.UserId == student.Id && e.CourseId == course.Id);
            Assert.That(enrollment, Is.Null);
        }

        [Test]
        public async Task ProcessWebhookPaymentAsync_PendingEvent_MarksPaymentPending()
        {
            var (student, course) = await SetupCourse(CourseAccessType.SelfPaced, isPremium: true);
            var orderId = "order_webhook_pending";
            var payId = "pay_webhook_pending";
            
            var payment = new Payments
            {
                UserId = student.Id,
                CourseId = course.Id,
                Status = PaymentStatus.Failed,
                ProviderOrderId = orderId,
                Amount = 100
            };
            DbContext.Payments.Add(payment);
            await DbContext.SaveChangesAsync();

            await _enrollmentService.ProcessWebhookPaymentAsync(orderId, payId, "payment.authorized", "{\"status\": \"authorized\"}");

            var updatedPayment = DbContext.Payments.FirstOrDefault(p => p.ProviderOrderId == orderId);
            Assert.That(updatedPayment!.Status, Is.EqualTo(PaymentStatus.Pending));
            Assert.That(updatedPayment.ProviderPaymentId, Is.EqualTo(payId));

            var enrollment = DbContext.Enrollments.FirstOrDefault(e => e.UserId == student.Id && e.CourseId == course.Id);
            Assert.That(enrollment, Is.Null);
        }
    }
}
