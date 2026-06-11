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
    public class BatchServiceTests : BaseServiceTest
    {
        private Mock<ILogger<BatchService>> _mockLogger = null!;
        private IBatchService _batchService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mockLogger = new Mock<ILogger<BatchService>>();
            
            var batchRepository = new CourseBatchRepository(DbContext);
            var courseRepository = new CourseRepository(DbContext);

            _batchService = new BatchService(
                batchRepository,
                courseRepository,
                Mapper,
                _mockLogger.Object
            );
        }

        private async Task<LMSApi.ModelLibrary.Models.Courses> CreateTestCourse(CourseAccessType accessType = CourseAccessType.CohortBased)
        {
            var cat = new CourseCategories { Name = "Cat", Description = "Desc" };
            var user = new Users { Email = "test@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Instructor", Description = "Desc" } };
            DbContext.CourseCategories.Add(cat);
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

            var course = new LMSApi.ModelLibrary.Models.Courses 
            { 
                Title = "Cohort Course", 
                Description = "Desc", 
                Price = 0m,
                ThumbnailUrl = "url",
                IntroVideoUrl = "url", 
                IsPremium = false,
                Requirements = "Reqs",
                LearningOutcomes = "Outcomes",
                EstimatedDuration = TimeSpan.Zero,
                Level = LMSApi.ModelLibrary.Enums.CourseLevel.Beginner,
                Language = LMSApi.ModelLibrary.Enums.CourseLanguage.English,
                PublishedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DefaultDeadlineDays = 7,
                CategoryId = cat.Id, 
                InstructorId = user.Id, 
                slug = Guid.NewGuid().ToString(),
                CourseAccessType = accessType
            };
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();
            return course;
        }

        [Test]
        public async Task CreateBatchAsync_ValidCohortCourse_CreatesBatch()
        {
            var course = await CreateTestCourse(CourseAccessType.CohortBased);

            var request = new CreateBatchRequest 
            { 
                Name = "Batch 1",
                StartDate = DateTime.UtcNow.AddDays(10),
                EndDate = DateTime.UtcNow.AddDays(20),
                EnrollmentStartDate = DateTime.UtcNow.AddDays(1),
                EnrollmentEndDate = DateTime.UtcNow.AddDays(5),
                MaxStudents = 100
            };

            var result = await _batchService.CreateBatchAsync(course.Id, request);

            Assert.That(result.Name, Is.EqualTo("Batch 1"));
            Assert.That(result.CourseId, Is.EqualTo(course.Id));
        }

        [Test]
        public async Task CreateBatchAsync_SelfPacedCourse_ThrowsInvalidOperationException()
        {
            var course = await CreateTestCourse(CourseAccessType.SelfPaced);

            var request = new CreateBatchRequest 
            { 
                Name = "Batch 1",
                StartDate = DateTime.UtcNow.AddDays(10),
                EndDate = DateTime.UtcNow.AddDays(20),
                EnrollmentStartDate = DateTime.UtcNow.AddDays(1),
                EnrollmentEndDate = DateTime.UtcNow.AddDays(5)
            };

            Assert.ThrowsAsync<InvalidOperationException>(() => _batchService.CreateBatchAsync(course.Id, request));
        }

        [Test]
        public async Task UpdateBatchAsync_ValidUpdate_UpdatesBatch()
        {
            var course = await CreateTestCourse();
            var batch = new CourseBatch
            {
                Name = "Old Batch",
                CourseId = course.Id,
                StartDate = DateTime.UtcNow.AddDays(10),
                EndDate = DateTime.UtcNow.AddDays(20),
                EnrollmentStartDate = DateTime.UtcNow.AddDays(1),
                EnrollmentEndDate = DateTime.UtcNow.AddDays(5),
                MaxStudents = 50,
                Status = BatchStatus.Upcoming
            };
            DbContext.CourseBatches.Add(batch);
            await DbContext.SaveChangesAsync();

            var updateReq = new UpdateBatchRequest { Name = "Updated Batch", MaxStudents = 60 };

            var result = await _batchService.UpdateBatchAsync(batch.Id, updateReq);

            Assert.That(result.Name, Is.EqualTo("Updated Batch"));
            Assert.That(result.MaxStudents, Is.EqualTo(60));
        }

        [Test]
        public async Task DeleteBatchAsync_DeletesBatch()
        {
            var course = await CreateTestCourse();
            var batch = new CourseBatch
            {
                Name = "To Delete",
                CourseId = course.Id,
                StartDate = DateTime.UtcNow.AddDays(10),
                EndDate = DateTime.UtcNow.AddDays(20),
                EnrollmentStartDate = DateTime.UtcNow.AddDays(1),
                EnrollmentEndDate = DateTime.UtcNow.AddDays(5),
                MaxStudents = 50,
                Status = BatchStatus.Upcoming
            };
            DbContext.CourseBatches.Add(batch);
            await DbContext.SaveChangesAsync();

            await _batchService.DeleteBatchAsync(batch.Id);

            var dbBatch = await DbContext.CourseBatches.FindAsync(batch.Id);
            Assert.That(dbBatch, Is.Null);
        }
    }
}
