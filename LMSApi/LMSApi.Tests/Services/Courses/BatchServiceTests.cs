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
    public class BatchServiceTests : BaseServiceTest
    {
        private Mock<ILogger<BatchService>> _mockLogger = null!;
        private Mock<INotificationService> _mockNotificationService = null!;
        private Mock<IUserNotificationsService> _mockUserNotificationsService = null!;
        private IBatchService _batchService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mockLogger = new Mock<ILogger<BatchService>>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockUserNotificationsService = new Mock<IUserNotificationsService>();
            
            var batchRepository = new CourseBatchRepository(DbContext);
            var courseRepository = new CourseRepository(DbContext);
            var enrollmentRepository = new EnrollmentRepository(DbContext);

            _batchService = new BatchService(
                batchRepository,
                courseRepository,
                Mapper,
                _mockLogger.Object,
                enrollmentRepository,
                _mockNotificationService.Object,
                _mockUserNotificationsService.Object
            );
        }

        private async Task<Courses> CreateTestCourse(CourseAccessType accessType = CourseAccessType.CohortBased)
        {
            var cat = new CourseCategories { Name = "Cat", Description = "Desc" };
            var user = new Users { Email = $"{Guid.NewGuid()}@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Instructor", Description = "Desc" } };
            DbContext.CourseCategories.Add(cat);
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

            var course = new Courses 
            { 
                Title = "Cohort Course", Description = "Desc", Price = 0m,
                ThumbnailUrl = "url", IntroVideoUrl = "url", IsPremium = false,
                Requirements = "Reqs", LearningOutcomes = "Outcomes",
                EstimatedDuration = TimeSpan.Zero,
                Level = CourseLevel.Beginner, LanguageId = 1,
                PublishedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
                DefaultDeadlineDays = 7, CategoryId = cat.Id, InstructorId = user.Id,
                slug = Guid.NewGuid().ToString(),
                CourseAccessType = accessType
            };
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();
            return course;
        }

        private CreateBatchRequest ValidBatchRequest(string name = "Batch 1") => new CreateBatchRequest
        {
            Name = name,
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(20),
            EnrollmentStartDate = DateTime.UtcNow.AddDays(1),
            EnrollmentEndDate = DateTime.UtcNow.AddDays(5),
            MaxStudents = 100
        };

        // ─── CreateBatchAsync ──────────────────────────────────────────────────

        [Test]
        public async Task CreateBatchAsync_ValidCohortCourse_CreatesBatch()
        {
            var course = await CreateTestCourse(CourseAccessType.CohortBased);

            var result = await _batchService.CreateBatchAsync(course.Id, ValidBatchRequest());

            Assert.That(result.Name, Is.EqualTo("Batch 1"));
            Assert.That(result.CourseId, Is.EqualTo(course.Id));
        }

        [Test]
        public async Task CreateBatchAsync_SelfPacedCourse_ThrowsInvalidOperationException()
        {
            var course = await CreateTestCourse(CourseAccessType.SelfPaced);

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _batchService.CreateBatchAsync(course.Id, ValidBatchRequest()));
        }

        [Test]
        public async Task CreateBatchAsync_EndDateBeforeStartDate_ThrowsArgumentException()
        {
            var course = await CreateTestCourse();
            var request = new CreateBatchRequest
            {
                Name = "BadDates",
                StartDate = DateTime.UtcNow.AddDays(20),
                EndDate = DateTime.UtcNow.AddDays(10), // end before start
                EnrollmentStartDate = DateTime.UtcNow.AddDays(1),
                EnrollmentEndDate = DateTime.UtcNow.AddDays(5),
                MaxStudents = 50
            };
            Assert.ThrowsAsync<ArgumentException>(() => _batchService.CreateBatchAsync(course.Id, request));
        }

        [Test]
        public async Task CreateBatchAsync_EnrollmentEndBeforeEnrollmentStart_ThrowsArgumentException()
        {
            var course = await CreateTestCourse();
            var request = new CreateBatchRequest
            {
                Name = "BadEnroll",
                StartDate = DateTime.UtcNow.AddDays(10),
                EndDate = DateTime.UtcNow.AddDays(20),
                EnrollmentStartDate = DateTime.UtcNow.AddDays(5),
                EnrollmentEndDate = DateTime.UtcNow.AddDays(1), // end before start
                MaxStudents = 50
            };
            Assert.ThrowsAsync<ArgumentException>(() => _batchService.CreateBatchAsync(course.Id, request));
        }

        [Test]
        public async Task CreateBatchAsync_EnrollmentWindowExtendsAfterBatchStart_ThrowsArgumentException()
        {
            var course = await CreateTestCourse();
            var request = new CreateBatchRequest
            {
                Name = "OverlapDates",
                StartDate = DateTime.UtcNow.AddDays(5),
                EndDate = DateTime.UtcNow.AddDays(20),
                EnrollmentStartDate = DateTime.UtcNow.AddDays(1),
                EnrollmentEndDate = DateTime.UtcNow.AddDays(8), // overlaps batch start (day 5)
                MaxStudents = 50
            };
            Assert.ThrowsAsync<ArgumentException>(() => _batchService.CreateBatchAsync(course.Id, request));
        }

        [Test]
        public void CreateBatchAsync_CourseNotFound_ThrowsKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _batchService.CreateBatchAsync(99999, ValidBatchRequest()));
        }

        // ─── UpdateBatchAsync ──────────────────────────────────────────────────

        [Test]
        public async Task UpdateBatchAsync_ValidUpdate_UpdatesBatch()
        {
            var course = await CreateTestCourse();
            var batch = new CourseBatch
            {
                Name = "Old Batch", CourseId = course.Id,
                StartDate = DateTime.UtcNow.AddDays(10), EndDate = DateTime.UtcNow.AddDays(20),
                EnrollmentStartDate = DateTime.UtcNow.AddDays(1), EnrollmentEndDate = DateTime.UtcNow.AddDays(5),
                MaxStudents = 50, Status = BatchStatus.Upcoming
            };
            DbContext.CourseBatches.Add(batch);
            await DbContext.SaveChangesAsync();

            var updateReq = new UpdateBatchRequest { Name = "Updated Batch", MaxStudents = 60 };

            var result = await _batchService.UpdateBatchAsync(batch.Id, updateReq);

            Assert.That(result.Name, Is.EqualTo("Updated Batch"));
            Assert.That(result.MaxStudents, Is.EqualTo(60));
        }

        [Test]
        public void UpdateBatchAsync_NotFound_ThrowsKeyNotFoundException()
        {
            var request = new UpdateBatchRequest { Name = "X" };
            Assert.ThrowsAsync<KeyNotFoundException>(() => _batchService.UpdateBatchAsync(99999, request));
        }

        [Test]
        public async Task UpdateBatchAsync_StatusChangedToActive_SendsEmailsToEnrolledLearners()
        {
            var course = await CreateTestCourse();
            var batch = new CourseBatch
            {
                Name = "Batch To Activate", CourseId = course.Id,
                StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(20),
                EnrollmentStartDate = DateTime.UtcNow.AddDays(-10), EnrollmentEndDate = DateTime.UtcNow.AddDays(-2),
                MaxStudents = 50, Status = BatchStatus.Upcoming
            };
            DbContext.CourseBatches.Add(batch);
            
            var student = new Users { Email = "bstudent@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Learner", Description = "Desc" } };
            DbContext.Users.Add(student);
            await DbContext.SaveChangesAsync();

            DbContext.Enrollments.Add(new Enrollments
            {
                CourseId = course.Id, UserId = student.Id, BatchId = batch.Id,
                EnrollmentStatus = EnrollmentStatus.Active, EnrolledAt = DateTime.UtcNow,
                ProgressPercentage = 0, IsCompleted = false
            });
            await DbContext.SaveChangesAsync();

            var updateReq = new UpdateBatchRequest { Status = BatchStatus.Active };
            await _batchService.UpdateBatchAsync(batch.Id, updateReq);

            await Task.Delay(100); // Give background task time to run
            _mockNotificationService.Verify(x => x.Send(It.IsAny<Message>()), Times.Once);
        }

        [Test]
        public async Task UpdateBatchAsync_StatusChangedToCompleted_SendsEmailsToEnrolledLearners()
        {
            var course = await CreateTestCourse();
            var batch = new CourseBatch
            {
                Name = "Batch To Complete", CourseId = course.Id,
                StartDate = DateTime.UtcNow.AddDays(-10), EndDate = DateTime.UtcNow.AddDays(-1),
                MaxStudents = 50, Status = BatchStatus.Active
            };
            DbContext.CourseBatches.Add(batch);
            
            var student = new Users { Email = "cstudent@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Learner", Description = "Desc" } };
            DbContext.Users.Add(student);
            await DbContext.SaveChangesAsync();

            DbContext.Enrollments.Add(new Enrollments
            {
                CourseId = course.Id, UserId = student.Id, BatchId = batch.Id,
                EnrollmentStatus = EnrollmentStatus.Active, EnrolledAt = DateTime.UtcNow,
                ProgressPercentage = 0, IsCompleted = false
            });
            await DbContext.SaveChangesAsync();

            var updateReq = new UpdateBatchRequest { Status = BatchStatus.Completed };
            await _batchService.UpdateBatchAsync(batch.Id, updateReq);

            await Task.Delay(100);
            _mockNotificationService.Verify(x => x.Send(It.IsAny<Message>()), Times.Once);
        }

        [Test]
        public async Task UpdateBatchAsync_StatusUnchanged_DoesNotSendEmails()
        {
            var course = await CreateTestCourse();
            var batch = new CourseBatch
            {
                Name = "Batch No Change", CourseId = course.Id,
                StartDate = DateTime.UtcNow.AddDays(-10), EndDate = DateTime.UtcNow.AddDays(10),
                MaxStudents = 50, Status = BatchStatus.Active
            };
            DbContext.CourseBatches.Add(batch);
            await DbContext.SaveChangesAsync();

            var updateReq = new UpdateBatchRequest { Status = BatchStatus.Active };
            await _batchService.UpdateBatchAsync(batch.Id, updateReq);

            await Task.Delay(100);
            _mockNotificationService.Verify(x => x.Send(It.IsAny<Message>()), Times.Never);
        }

        // ─── DeleteBatchAsync ──────────────────────────────────────────────────

        [Test]
        public async Task DeleteBatchAsync_DeletesBatch()
        {
            var course = await CreateTestCourse();
            var batch = new CourseBatch
            {
                Name = "To Delete", CourseId = course.Id,
                StartDate = DateTime.UtcNow.AddDays(10), EndDate = DateTime.UtcNow.AddDays(20),
                EnrollmentStartDate = DateTime.UtcNow.AddDays(1), EnrollmentEndDate = DateTime.UtcNow.AddDays(5),
                MaxStudents = 50, Status = BatchStatus.Upcoming
            };
            DbContext.CourseBatches.Add(batch);
            await DbContext.SaveChangesAsync();

            await _batchService.DeleteBatchAsync(batch.Id);

            var dbBatch = await DbContext.CourseBatches.FindAsync(batch.Id);
            Assert.That(dbBatch, Is.Null);
        }

        [Test]
        public void DeleteBatchAsync_NotFound_ThrowsKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _batchService.DeleteBatchAsync(99999));
        }

        // ─── GetBatchByIdAsync / GetBatchesByCourseAsync ───────────────────────

        [Test]
        public async Task GetBatchByIdAsync_ExistingBatch_ReturnsBatch()
        {
            var course = await CreateTestCourse();
            var batch = new CourseBatch
            {
                Name = "FindMe", CourseId = course.Id,
                StartDate = DateTime.UtcNow.AddDays(10), EndDate = DateTime.UtcNow.AddDays(20),
                EnrollmentStartDate = DateTime.UtcNow.AddDays(1), EnrollmentEndDate = DateTime.UtcNow.AddDays(5),
                MaxStudents = 30, Status = BatchStatus.Upcoming
            };
            DbContext.CourseBatches.Add(batch);
            await DbContext.SaveChangesAsync();

            var result = await _batchService.GetBatchByIdAsync(batch.Id);
            Assert.That(result.Name, Is.EqualTo("FindMe"));
        }

        [Test]
        public async Task GetBatchesByCourseAsync_NoBatches_ReturnsEmpty()
        {
            var result = await _batchService.GetBatchesByCourseAsync(99999);
            Assert.That(result, Is.Empty);
        }
    }
}
