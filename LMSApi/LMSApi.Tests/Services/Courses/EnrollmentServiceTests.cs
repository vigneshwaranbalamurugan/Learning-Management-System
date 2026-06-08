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

namespace LMSApi.Tests.Services.Courses
{
    [TestFixture]
    public class EnrollmentServiceTests : BaseServiceTest
    {
        private Mock<ILogger<EnrollmentService>> _mockLogger = null!;
        private Mock<IPaymentService> _mockPaymentService = null!;
        private IEnrollmentService _enrollmentService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mockLogger = new Mock<ILogger<EnrollmentService>>();
            _mockPaymentService = new Mock<IPaymentService>();
            
            var enrollmentRepository = new EnrollmentRepository(DbContext);
            var courseRepository = new CourseRepository(DbContext);
            var batchRepository = new CourseBatchRepository(DbContext);
            var paymentRepository = new PaymentRepository(DbContext);

            _enrollmentService = new EnrollmentService(
                enrollmentRepository,
                courseRepository,
                batchRepository,
                paymentRepository,
                Mapper,
                _mockLogger.Object,
                _mockPaymentService.Object
            );
        }

        private async Task<(Users student, LMSApi.ModelLibrary.Models.Courses course)> SetupCourse(CourseAccessType type)
        {
            var student = new Users { Email = "student@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Learner", Description = "Desc" } };
            var cat = new CourseCategories { Name = "Cat", Description = "Desc" };
            var inst = new Users { Email = "inst@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Instructor", Description = "Desc" } };
            DbContext.Users.Add(student);
            DbContext.Users.Add(inst);
            DbContext.CourseCategories.Add(cat);
            await DbContext.SaveChangesAsync();

            var course = new LMSApi.ModelLibrary.Models.Courses
            {
                Title = "Course",
                Description = "Desc",
                Price = 0m,
                ThumbnailUrl = "url",
                IntroVideoUrl = "url",
                IsPremium = false,
                IsPublished = false,
                Requirements = "Reqs",
                LearningOutcomes = "Outcomes",
                EstimatedDuration = TimeSpan.Zero,
                Level = LMSApi.ModelLibrary.Enums.CourseLevel.Beginner,
                Language = LMSApi.ModelLibrary.Enums.CourseLanguage.English,
                PublishedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DefaultAssignmentDeadlineDays = 7,
                CategoryId = cat.Id,
                InstructorId = inst.Id,
                slug = Guid.NewGuid().ToString(),
                Status = CourseStatus.Published,
                CourseAccessType = type,
            };
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            return (student, course);
        }

        [Test]
        public async Task EnrollInFreeCourseAsync_SelfPaced_CreatesEnrollment()
        {
            var (student, course) = await SetupCourse(CourseAccessType.SelfPaced);
            
            var result = await _enrollmentService.EnrollInFreeCourseAsync(student.Id, course.Id, null);

            Assert.That(result.CourseId, Is.EqualTo(course.Id));
            Assert.That(result.UserId, Is.EqualTo(student.Id));
            Assert.That(result.BatchId, Is.Null);
        }

        [Test]
        public async Task EnrollInFreeCourseAsync_CohortBasedWithoutBatchId_ThrowsException()
        {
            var (student, course) = await SetupCourse(CourseAccessType.CohortBased);
            
            Assert.ThrowsAsync<InvalidOperationException>(() => _enrollmentService.EnrollInFreeCourseAsync(student.Id, course.Id, null));
        }

        // Removed UpdateEnrollmentStatus test since method is missing in IEnrollmentService

        [Test]
        public async Task ValidateCourseAccessAsync_ActiveEnrollment_ReturnsTrue()
        {
            var (student, course) = await SetupCourse(CourseAccessType.SelfPaced);
            var enrollment = new Enrollments { CourseId = course.Id, UserId = student.Id, EnrollmentStatus = EnrollmentStatus.Active, EnrolledAt = DateTime.UtcNow, ProgressPercentage = 0, IsCompleted = false };
            DbContext.Enrollments.Add(enrollment);
            await DbContext.SaveChangesAsync();

            var hasAccess = await _enrollmentService.ValidateCourseAccessAsync(enrollment.Id);

            Assert.That(hasAccess, Is.True);
        }
    }
}
