using System.Linq;
using System.Threading.Tasks;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Services;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LMSApi.Tests.Services
{
    [TestFixture]
    public class PaymentServiceTests
    {
        private Mock<IPaymentProvider> _mockRazorpayProvider = null!;
        private Mock<IPaymentRepository> _mockPaymentRepo = null!;
        private Mock<IPlatformFeeService> _mockFeeService = null!;
        private Mock<ICourseRepository> _mockCourseRepo = null!;
        private Mock<ILogger<PaymentService>> _mockLogger = null!;
        private PaymentService _paymentService = null!;

        [SetUp]
        public void SetUp()
        {
            _mockRazorpayProvider = new Mock<IPaymentProvider>();
            _mockPaymentRepo = new Mock<IPaymentRepository>();
            _mockFeeService = new Mock<IPlatformFeeService>();
            _mockCourseRepo = new Mock<ICourseRepository>();
            _mockLogger = new Mock<ILogger<PaymentService>>();
            
            _mockRazorpayProvider.Setup(p => p.ProviderName).Returns("Razorpay");

            var providers = new[] { _mockRazorpayProvider.Object };

            _paymentService = new PaymentService(
                providers,
                _mockPaymentRepo.Object,
                _mockFeeService.Object,
                _mockCourseRepo.Object,
                _mockLogger.Object
            );
        }

        [Test]
        public async Task CreateCourseOrderAsync_ValidProvider_CreatesOrderAndReturnsOrderId()
        {
            // Arrange
            int courseId = 2;
            decimal amount = 199.99m;
            string expectedOrderId = "order_12345";

            _mockCourseRepo.Setup(r => r.GetByIdAsync(courseId))
                .ReturnsAsync(new Courses { Id = courseId, Price = amount });
            
            _mockFeeService.Setup(f => f.CalculateSplitAsync(amount, FeeCategory.CourseFee, It.IsAny<System.DateTime>()))
                .ReturnsAsync((10m, 189.99m, new PlatformFeeConfig { Id = 1 }));

            _mockRazorpayProvider
                .Setup(p => p.CreateOrderAsync(amount, "INR", It.IsAny<string>()))
                .ReturnsAsync(expectedOrderId);

            // Act
            var result = await _paymentService.CreateCourseOrderAsync(courseId, "INR");

            // Assert
            Assert.That(result.orderId, Is.EqualTo(expectedOrderId));
            _mockRazorpayProvider.Verify(p => p.CreateOrderAsync(amount, "INR", It.Is<string>(r => r.StartsWith($"rcpt_course_{courseId}_"))), Times.Once);
        }

        [Test]
        public async Task CreateCourseOrderAsync_ValidProvider_LogsInformation()
        {
            // Arrange
            int courseId = 2;
            decimal amount = 199.99m;
            string expectedOrderId = "order_12345";

            _mockCourseRepo.Setup(r => r.GetByIdAsync(courseId))
                .ReturnsAsync(new Courses { Id = courseId, Price = amount });
            
            _mockFeeService.Setup(f => f.CalculateSplitAsync(amount, FeeCategory.CourseFee, It.IsAny<System.DateTime>()))
                .ReturnsAsync((10m, 189.99m, new PlatformFeeConfig { Id = 1 }));

            _mockRazorpayProvider
                .Setup(p => p.CreateOrderAsync(amount, "INR", It.IsAny<string>()))
                .ReturnsAsync(expectedOrderId);

            // Act
            await _paymentService.CreateCourseOrderAsync(courseId, "INR");

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating course order for Course ID")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Calculated split for Course ID")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Course order created successfully")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public void CreateCourseOrderAsync_FreeCourse_ThrowsInvalidOperationException()
        {
            // Arrange
            int courseId = 2;
            
            _mockCourseRepo.Setup(r => r.GetByIdAsync(courseId))
                .ReturnsAsync(new Courses { Id = courseId, Price = 0m });

            // Act & Assert
            var ex = Assert.ThrowsAsync<System.InvalidOperationException>(() => 
                _paymentService.CreateCourseOrderAsync(courseId, "INR"));
            
            Assert.That(ex.Message, Does.Contain("Cannot create an order for a free course"));
        }

        [Test]
        public void CreateCourseOrderAsync_FreeCourse_LogsWarning()
        {
            // Arrange
            int courseId = 2;
            
            _mockCourseRepo.Setup(r => r.GetByIdAsync(courseId))
                .ReturnsAsync(new Courses { Id = courseId, Price = 0m });

            // Act & Assert
            Assert.ThrowsAsync<System.InvalidOperationException>(() => 
                _paymentService.CreateCourseOrderAsync(courseId, "INR"));

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to create course order: Course ID")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public void VerifySignature_ValidSignature_ReturnsTrue()
        {
            // Arrange
            string providerName = "Razorpay";
            string orderId = "order_123";
            string paymentId = "pay_123";
            string signature = "sig_123";

            _mockRazorpayProvider
                .Setup(p => p.VerifySignature(orderId, paymentId, signature))
                .Returns(true);

            // Act
            var result = _paymentService.VerifySignature(providerName, orderId, paymentId, signature);

            // Assert
            Assert.That(result, Is.True);
            _mockRazorpayProvider.Verify(p => p.VerifySignature(orderId, paymentId, signature), Times.Once);
        }

        [Test]
        public void VerifySignature_InvalidProvider_ThrowsNotSupportedException()
        {
            // Arrange
            string providerName = "InvalidProvider";
            string orderId = "order_123";
            string paymentId = "pay_123";
            string signature = "sig_123";

            // Act & Assert
            var ex = Assert.Throws<System.NotSupportedException>(() => 
                _paymentService.VerifySignature(providerName, orderId, paymentId, signature));
            
            Assert.That(ex.Message, Does.Contain("Payment provider 'InvalidProvider' is not supported"));
        }

        [Test]
        public async Task ConfirmPaymentAsync_ValidSignature_ConfirmsPaymentAndLogsInfo()
        {
            // Arrange
            string orderId = "order_123";
            string paymentId = "pay_123";
            string sig = "sig_123";
            var existingPayment = new Payments
            {
                Id = 1,
                ProviderOrderId = orderId,
                Status = PaymentStatus.Pending
            };

            _mockRazorpayProvider
                .Setup(p => p.VerifySignature(orderId, paymentId, sig))
                .Returns(true);

            _mockPaymentRepo
                .Setup(r => r.GetByProviderOrderIdAsync(orderId))
                .ReturnsAsync(existingPayment);

            // Act
            var result = await _paymentService.ConfirmPaymentAsync(
                orderId, paymentId, sig, 1, 2, 3, 1, 10m, 189.99m, FeeType.Percentage, 5m);

            // Assert
            Assert.That(result.Status, Is.EqualTo(PaymentStatus.Completed));
            _mockPaymentRepo.Verify(r => r.UpdateAsync(It.Is<Payments>(p => p.ProviderPaymentId == paymentId && p.Status == PaymentStatus.Completed)), Times.Once);
            
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Confirming payment for Provider Order ID")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Payment confirmed and updated in database")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public void ConfirmPaymentAsync_InvalidSignature_ThrowsUnauthorizedAccessExceptionAndLogsWarning()
        {
            // Arrange
            string orderId = "order_123";
            string paymentId = "pay_123";
            string sig = "sig_123";

            _mockRazorpayProvider
                .Setup(p => p.VerifySignature(orderId, paymentId, sig))
                .Returns(false);

            // Act & Assert
            Assert.ThrowsAsync<System.UnauthorizedAccessException>(() =>
                _paymentService.ConfirmPaymentAsync(
                    orderId, paymentId, sig, 1, 2, 3, 1, 10m, 189.99m, FeeType.Percentage, 5m));

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Payment confirmation failed: Signature verification failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
