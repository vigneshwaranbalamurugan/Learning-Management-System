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
    public class AssignmentSubmissionServiceTests : BaseServiceTest
    {
        private Mock<IUploadService> _mockUploadService = null!;
        private Mock<INotificationService> _mockNotificationService = null!;
        private Mock<ILogger<AssignmentSubmissionService>> _mockLogger = null!;
        private IAssignmentSubmissionService _submissionService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mockUploadService = new Mock<IUploadService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockLogger = new Mock<ILogger<AssignmentSubmissionService>>();

            var assignmentRepo = new AssignmentRepository(DbContext);
            var submissionRepo = new AssignmentSubmissionRepository(DbContext);
            var sectionRepo = new CourseSectionRepository(DbContext);
            var enrollmentRepo = new EnrollmentRepository(DbContext);
            var courseRepo = new CourseRepository(DbContext);
            var progressService = new Mock<IStudentProgressService>();
            var userRepo = new UserRepository(DbContext);

            _submissionService = new AssignmentSubmissionService(
                assignmentRepo,
                submissionRepo,
                sectionRepo,
                enrollmentRepo,
                courseRepo,
                progressService.Object,
                _mockUploadService.Object,
                Mapper,
                _mockLogger.Object,
                _mockNotificationService.Object,
                userRepo
            );
        }

        // ─── SubmitAssignmentAsync ─────────────────────────────────────────────

        [Test]
        public async Task SubmitAssignmentAsync_NotEnrolled_ThrowsUnauthorizedAccessException()
        {
            var (_, section, _) = await SetupCourseAndSection(CourseAccessType.CohortBased);
            var assignment = new Assignments { Title = "A", CourseSectionId = section.Id, Status = PublishStatus.Published, TotalMarks = 10 };
            DbContext.Assignments.Add(assignment);
            await DbContext.SaveChangesAsync();

            var req = new AssignmentSubmissionRequest { AssignmentId = assignment.Id, SubmittedAssignmentUrl = "http" };
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => _submissionService.SubmitAssignmentAsync(9999, req));
        }

        [Test]
        public async Task SubmitAssignmentAsync_ValidSubmission_CreatesSubmission()
        {
            var (_, section, course) = await SetupCourseAndSection(CourseAccessType.CohortBased);
            var assignment = new Assignments { Title = "A", CourseSectionId = section.Id, Status = PublishStatus.Published, TotalMarks = 10 };
            DbContext.Assignments.Add(assignment);

            var student = new Users { Email = "substudent@test.com", PasswordHash = "h", PasswordSalt = "s", Role = new UserRoles { RoleName = "Learner", Description = "Desc" } };
            DbContext.Users.Add(student);
            await DbContext.SaveChangesAsync();

            DbContext.Enrollments.Add(new Enrollments
            {
                CourseId = course.Id,
                UserId = student.Id,
                EnrollmentStatus = EnrollmentStatus.Active,
                EnrolledAt = DateTime.UtcNow,
                ProgressPercentage = 0,
                IsCompleted = false
            });
            await DbContext.SaveChangesAsync();

            var req = new AssignmentSubmissionRequest { AssignmentId = assignment.Id, SubmittedAssignmentUrl = "http" };
            var result = await _submissionService.SubmitAssignmentAsync(student.Id, req);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo(SubmissionStatus.Submitted.ToString()));
        }

        // ─── GradeAssignmentAsync ──────────────────────────────────────────────

        [Test]
        public async Task GradeAssignmentAsync_InvalidMarksExceedTotal_ThrowsArgumentException()
        {
            var (_, section, _) = await SetupCourseAndSection(CourseAccessType.CohortBased);
            var assignment = new Assignments { Title = "A", CourseSectionId = section.Id, Status = PublishStatus.Published, TotalMarks = 10 };
            DbContext.Assignments.Add(assignment);
            await DbContext.SaveChangesAsync(); // must save assignment first so FK is valid

            var sub = new AssignmentSubmissions { AssignmentId = assignment.Id, StudentId = 1, Status = SubmissionStatus.Submitted };
            DbContext.AssignmentSubmissions.Add(sub);
            await DbContext.SaveChangesAsync();

            var req = new GradeSubmissionRequest { MarksAwarded = 11, Feedback = "Good" };
            Assert.ThrowsAsync<ArgumentException>(() => _submissionService.GradeAssignmentAsync(sub.Id, req));
        }

        [Test]
        public async Task GradeAssignmentAsync_ValidGrade_GradesSubmissionAndSendsEmail()
        {
            var (_, section, course) = await SetupCourseAndSection(CourseAccessType.CohortBased);
            var assignment = new Assignments { Title = "A", CourseSectionId = section.Id, Status = PublishStatus.Published, TotalMarks = 10 };
            DbContext.Assignments.Add(assignment);

            var student = new Users { Email = "gradestudent@test.com", PasswordHash = "h", PasswordSalt = "s", Role = new UserRoles { RoleName = "Learner", Description = "Desc" } };
            DbContext.Users.Add(student);
            await DbContext.SaveChangesAsync();

            DbContext.Enrollments.Add(new Enrollments
            {
                CourseId = course.Id,
                UserId = student.Id,
                EnrollmentStatus = EnrollmentStatus.Active,
                EnrolledAt = DateTime.UtcNow,
                ProgressPercentage = 0,
                IsCompleted = false
            });

            var sub = new AssignmentSubmissions { AssignmentId = assignment.Id, StudentId = student.Id, Status = SubmissionStatus.Submitted };
            DbContext.AssignmentSubmissions.Add(sub);
            await DbContext.SaveChangesAsync();

            var req = new GradeSubmissionRequest { MarksAwarded = 8, Feedback = "Good" };
            var result = await _submissionService.GradeAssignmentAsync(sub.Id, req);

            Assert.That(result.Status, Is.EqualTo(SubmissionStatus.Graded.ToString()));
            Assert.That(result.MarksAwarded, Is.EqualTo(8));

            await Task.Delay(100);
            _mockNotificationService.Verify(x => x.Send(It.IsAny<Message>()), Times.Once);
        }

        [Test]
        public async Task SubmitAssignmentAsync_CohortBased_AfterDeadlineDate_ThrowsInvalidOperationException()
        {
            var (_, section, course) = await SetupCourseAndSection(CourseAccessType.CohortBased);

            var assignment = new Assignments
            {
                Title = "Past Deadline Assignment",
                CourseSectionId = section.Id,
                TotalMarks = 100,
                DeadlineDate = DateTime.UtcNow.AddDays(-2), // past
                IsLateSubmissionAllowed = false,
                MaxSubmissions = 1
            };
            DbContext.Assignments.Add(assignment);

            var student = new Users { Email = "latudent@test.com", PasswordHash = "h", PasswordSalt = "s", Role = new UserRoles { RoleName = "Learner", Description = "Desc" } };
            DbContext.Users.Add(student);
            await DbContext.SaveChangesAsync();

            DbContext.Enrollments.Add(new Enrollments
            {
                CourseId = course.Id,
                UserId = student.Id,
                EnrollmentStatus = EnrollmentStatus.Active,
                EnrolledAt = DateTime.UtcNow.AddDays(-5),
                ProgressPercentage = 0,
                IsCompleted = false
            });
            await DbContext.SaveChangesAsync();

            var req = new AssignmentSubmissionRequest { AssignmentId = assignment.Id, SubmittedAssignmentUrl = "http" };
            Assert.ThrowsAsync<InvalidOperationException>(() => _submissionService.SubmitAssignmentAsync(student.Id, req));
        }
    }
}