using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Services;

using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Moq;
using NUnit.Framework;

namespace LMSApi.Tests.Services
{
    [TestFixture]
    public class RevenueServiceTests
    {
        private Mock<IInstructorPayoutService> _mockPayoutService;
        private Mock<IPaymentRepository> _mockPaymentRepository;
        private IMapper _mapper;
        private RevenueService _revenueService;

        [SetUp]
        public void SetUp()
        {
            _mockPayoutService = new Mock<IInstructorPayoutService>();
            _mockPaymentRepository = new Mock<IPaymentRepository>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(typeof(LMSApi.BALLibrary.Services.AuthService).Assembly);
            });
            _mapper = config.CreateMapper();

            _revenueService = new RevenueService(
                _mockPayoutService.Object,
                _mockPaymentRepository.Object,
                _mapper
            );
        }

        [Test]
        public async Task GetInstructorRevenueSummaryAsync_CalculatesCorrectTotals()
        {
            // Arrange
            int instructorId = 123;
            var instructorPayments = new List<ModelLibrary.Models.Payments>
            {
                new ModelLibrary.Models.Payments { InstructorAmount = 100m, Status = PaymentStatus.Completed },
                new ModelLibrary.Models.Payments { InstructorAmount = 150m, Status = PaymentStatus.Transferred }
            };

            var payouts = new List<InstructorPayout>
            {
                new InstructorPayout { Amount = 100m, Status = PayoutStatus.Processed },
                new InstructorPayout { Amount = 50m, Status = PayoutStatus.Pending },
                new InstructorPayout { Amount = 30m, Status = PayoutStatus.Failed }
            };

            _mockPaymentRepository.Setup(r => r.GetPaymentsByInstructorAsync(instructorId))
                .ReturnsAsync(instructorPayments);

            _mockPayoutService.Setup(s => s.GetPayoutsForInstructorAsync(instructorId))
                .ReturnsAsync(payouts);

            // Act
            var result = await _revenueService.GetInstructorRevenueSummaryAsync(instructorId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.InstructorId, Is.EqualTo(instructorId));
            Assert.That(result.TotalEarned, Is.EqualTo(100m)); // Only processed counts
            Assert.That(result.PendingAmount, Is.EqualTo(150m)); // 250 (total share) - 100 (processed)
            Assert.That(result.TotalPayouts, Is.EqualTo(3));
            Assert.That(result.Payouts.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task GetAdminRevenueDashboardAsync_CalculatesCorrectDashboardTotals()
        {
            // Arrange
            var allPayouts = new List<InstructorPayout>
            {
                new InstructorPayout { InstructorId = 1, Amount = 200m, Status = PayoutStatus.Processed },
                new InstructorPayout { InstructorId = 2, Amount = 100m, Status = PayoutStatus.Processed },
                new InstructorPayout { InstructorId = 2, Amount = 50m, Status = PayoutStatus.Failed }
            };

            var course1 = new Courses { InstructorId = 1 };
            var course2 = new Courses { InstructorId = 2 };

            var completedPayments = new List<ModelLibrary.Models.Payments>
            {
                new ModelLibrary.Models.Payments { Course = course1, Amount = 300m, PlatformFeeAmount = 30m, InstructorAmount = 270m, Status = PaymentStatus.Completed },
                new ModelLibrary.Models.Payments { Course = course2, Amount = 150m, PlatformFeeAmount = 15m, InstructorAmount = 135m, Status = PaymentStatus.Transferred }
            };

            var pendingManualReviews = new List<InstructorPayout>
            {
                new InstructorPayout { InstructorId = 2, Amount = 50m, Status = PayoutStatus.Failed, Notes = "Manual Review Needed" }
            };

            _mockPayoutService.Setup(s => s.GetAllPayoutsAsync())
                .ReturnsAsync(allPayouts);

            _mockPaymentRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(completedPayments);

            _mockPayoutService.Setup(s => s.GetPendingManualReviewAsync())
                .ReturnsAsync(pendingManualReviews);

            // Act
            var result = await _revenueService.GetAdminRevenueDashboardAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalRevenue, Is.EqualTo(450m)); // 300 + 150
            Assert.That(result.TotalPlatformFees, Is.EqualTo(45m)); // 30 + 15
            Assert.That(result.TotalInstructorPayouts, Is.EqualTo(300m)); // 200 + 100 processed payouts
            Assert.That(result.TotalTransactions, Is.EqualTo(2));
            Assert.That(result.PendingManualReviews.Count, Is.EqualTo(1));
            Assert.That(result.ByInstructor.Count, Is.EqualTo(2));

            var instructor1Summary = result.ByInstructor.First(i => i.InstructorId == 1);
            Assert.That(instructor1Summary.TotalEarned, Is.EqualTo(200m));
            Assert.That(instructor1Summary.PendingAmount, Is.EqualTo(70m)); // 270 share - 200 processed

            var instructor2Summary = result.ByInstructor.First(i => i.InstructorId == 2);
            Assert.That(instructor2Summary.TotalEarned, Is.EqualTo(100m));
            Assert.That(instructor2Summary.PendingAmount, Is.EqualTo(35m)); // 135 share - 100 processed
        }
    }
}
