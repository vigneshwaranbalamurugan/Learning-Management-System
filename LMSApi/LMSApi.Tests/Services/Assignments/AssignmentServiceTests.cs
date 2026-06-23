using System;
using System.Collections.Generic;
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
    public class AssignmentServiceTests : BaseServiceTest
    {
        private Mock<ILogger<AssignmentService>> _mockLogger = null!;
        private Mock<IUploadService> _mockUploadService = null!;
        private Mock<INotificationService> _mockNotificationService = null!;
        private Mock<IUserNotificationsService> _mockUserNotificationsService = null!;
        private IAssignmentService _assignmentService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mockLogger = new Mock<ILogger<AssignmentService>>();
            _mockUploadService = new Mock<IUploadService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockUserNotificationsService = new Mock<IUserNotificationsService>();

            var assignmentRepo = new AssignmentRepository(DbContext);
            var sectionRepo = new CourseSectionRepository(DbContext);
            var courseRepo = new CourseRepository(DbContext);
            var enrollmentRepo = new EnrollmentRepository(DbContext);
            var batchRepo = new CourseBatchRepository(DbContext);

            _assignmentService = new AssignmentService(
                assignmentRepo,
                sectionRepo,
                enrollmentRepo,
                courseRepo,
                _mockUploadService.Object,
                Mapper,
                _mockLogger.Object,
                _mockNotificationService.Object,
                batchRepo,
                _mockUserNotificationsService.Object
            );
        }


        // ─── CreateAssignmentAsync ─────────────────────────────────────────────

        [Test]
        public async Task CreateAssignmentAsync_ValidRequest_CreatesAssignment()
        {
            var (_, section, _) = await SetupCourseAndSection();
            var req = new CreateAssignmentRequest { CourseSectionId = section.Id, Title = "A", Description = "D", TotalMarks = 100, DeadlineInDays = 7 };

            var result = await _assignmentService.CreateAssignmentAsync(req);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Title, Is.EqualTo("A"));
        }

        [Test]
        public void CreateAssignmentAsync_EmptyTitle_ThrowsArgumentException()
        {
            var req = new CreateAssignmentRequest { Title = "", TotalMarks = 10 };
            Assert.ThrowsAsync<ArgumentException>(() => _assignmentService.CreateAssignmentAsync(req));
        }

        [Test]
        public void CreateAssignmentAsync_InvalidMarks_ThrowsArgumentException()
        {
            var req = new CreateAssignmentRequest { Title = "A", TotalMarks = 0 };
            Assert.ThrowsAsync<ArgumentException>(() => _assignmentService.CreateAssignmentAsync(req));
        }

        // ─── PublishAssignmentAsync ────────────────────────────────────────────

        [Test]
        public async Task PublishAssignmentAsync_SelfPaced_ThrowsInvalidOperationException()
        {
            var (_, section, _) = await SetupCourseAndSection(CourseAccessType.SelfPaced);
            var assignment = new Assignments { Title = "A", CourseSectionId = section.Id, Status = PublishStatus.Draft, TotalMarks = 10 };
            DbContext.Assignments.Add(assignment);
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() => _assignmentService.PublishAssignmentAsync(assignment.Id, true));
        }

        [Test]
        public async Task PublishAssignmentAsync_CohortBased_PublishesAssignment()
        {
            var (_, section, _) = await SetupCourseAndSection(CourseAccessType.CohortBased);
            var assignment = new Assignments { Title = "A", CourseSectionId = section.Id, Status = PublishStatus.Draft, TotalMarks = 10 };
            DbContext.Assignments.Add(assignment);
            await DbContext.SaveChangesAsync();

            var result = await _assignmentService.PublishAssignmentAsync(assignment.Id, true);
            Assert.That(result.Status, Is.EqualTo(PublishStatus.Published));
        }

        [Test]
        public async Task PublishAssignmentAsync_CohortBased_WithEnrolledLearners_SendsNotificationEmails()
        {
            var (_, section, course) = await SetupCourseAndSection(CourseAccessType.CohortBased);
            var assignment = new Assignments { Title = "A", CourseSectionId = section.Id, Status = PublishStatus.Draft, TotalMarks = 10 };
            DbContext.Assignments.Add(assignment);

            var student = new Users { Email = "astudent@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Learner", Description = "Desc" } };
            DbContext.Users.Add(student);
            await DbContext.SaveChangesAsync();

            DbContext.Enrollments.Add(new Enrollments
            {
                CourseId = course.Id, UserId = student.Id,
                EnrollmentStatus = EnrollmentStatus.Active, EnrolledAt = DateTime.UtcNow,
                ProgressPercentage = 0, IsCompleted = false
            });
            await DbContext.SaveChangesAsync();

            await _assignmentService.PublishAssignmentAsync(assignment.Id, true);
            
            await Task.Delay(100);
            _mockNotificationService.Verify(x => x.Send(It.IsAny<Message>()), Times.Once);
        }

        // ─── Cohort Deadline Date and Validation Tests ────────────────────────

        [Test]
        public async Task CreateAssignmentAsync_CohortBased_DeadlineDateRequired_ThrowsArgumentException()
        {
            var (_, section, _) = await SetupCourseAndSection(CourseAccessType.CohortBased);
            var req = new CreateAssignmentRequest
            {
                CourseSectionId = section.Id,
                Title = "Cohort Assignment No Deadline",
                TotalMarks = 100,
                DeadlineDate = null
            };

            Assert.ThrowsAsync<ArgumentException>(() => _assignmentService.CreateAssignmentAsync(req));
        }

        [Test]
        public async Task CreateAssignmentAsync_CohortBased_DeadlineDateExceedsBatchEndDate_ThrowsArgumentException()
        {
            var (_, section, course) = await SetupCourseAndSection(CourseAccessType.CohortBased);
            
            // Add a batch to the course
            var batch = new CourseBatch
            {
                CourseId = course.Id,
                Name = "Batch A",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(5),
                MaxStudents = 10,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            DbContext.CourseBatches.Add(batch);
            await DbContext.SaveChangesAsync();

            var req = new CreateAssignmentRequest
            {
                CourseSectionId = section.Id,
                Title = "Cohort Assignment Exceeds",
                TotalMarks = 100,
                DeadlineDate = DateTime.UtcNow.AddDays(6) // Exceeds 5 days
            };

            Assert.ThrowsAsync<ArgumentException>(() => _assignmentService.CreateAssignmentAsync(req));
        }

        [Test]
        public async Task CreateAssignmentAsync_CohortBased_ValidDeadlineDate_CreatesAssignment()
        {
            var (_, section, course) = await SetupCourseAndSection(CourseAccessType.CohortBased);
            
            var batch = new CourseBatch
            {
                CourseId = course.Id,
                Name = "Batch B",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(5),
                MaxStudents = 10,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            DbContext.CourseBatches.Add(batch);
            await DbContext.SaveChangesAsync();

            var req = new CreateAssignmentRequest
            {
                CourseSectionId = section.Id,
                Title = "Cohort Assignment Valid",
                TotalMarks = 100,
                DeadlineDate = DateTime.UtcNow.AddDays(4)
            };

            var result = await _assignmentService.CreateAssignmentAsync(req);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.DeadlineDate, Is.EqualTo(req.DeadlineDate));
        }

        [Test]
        public async Task UpdateAssignmentAsync_CohortBased_DeadlineDateExceedsBatchEndDate_ThrowsArgumentException()
        {
            var (_, section, course) = await SetupCourseAndSection(CourseAccessType.CohortBased);
            
            var batch = new CourseBatch
            {
                CourseId = course.Id,
                Name = "Batch C",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(5),
                MaxStudents = 10,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            DbContext.CourseBatches.Add(batch);
            await DbContext.SaveChangesAsync();

            var assignment = new Assignments
            {
                Title = "Original Assignment",
                CourseSectionId = section.Id,
                TotalMarks = 100,
                DeadlineDate = DateTime.UtcNow.AddDays(2)
            };
            DbContext.Assignments.Add(assignment);
            await DbContext.SaveChangesAsync();

            var req = new UpdateAssignmentRequest
            {
                DeadlineDate = DateTime.UtcNow.AddDays(10) // exceeds
            };

            Assert.ThrowsAsync<ArgumentException>(() => _assignmentService.UpdateAssignmentAsync(assignment.Id, req));
        }

    }
}
