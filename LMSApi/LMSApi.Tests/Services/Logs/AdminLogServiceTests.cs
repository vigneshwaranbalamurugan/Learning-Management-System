using System;
using System.Linq;
using System.Threading.Tasks;
using LMSApi.BALLibrary.Services;
using LMSApi.DALLibrary.Repositories;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace LMSApi.Tests.Services
{
    [TestFixture]
    public class AdminLogServiceTests : BaseServiceTest
    {
        private ActivityLogsRepository _activityLogsRepository = null!;
        private AuditLogsRepository _auditLogsRepository = null!;
        private AdminLogService _adminLogService = null!;
        private Users _testUser = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            // Set up test user
            _testUser = new Users
            {
                Email = "loguser@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                IsActive = true,
                IsEmailVerified = true,
                Role = new UserRoles { RoleName = "Student", Description = "Student" }
            };
            DbContext.Users.Add(_testUser);
            DbContext.SaveChanges();

            _activityLogsRepository = new ActivityLogsRepository(DbContext);
            _auditLogsRepository = new AuditLogsRepository(DbContext);
            _adminLogService = new AdminLogService(_activityLogsRepository, _auditLogsRepository, Mapper);
        }

        [Test]
        public async Task SaveChanges_AuditsDatabaseInsertUpdateDelete()
        {
            // ─── 1. INSERT AUDIT ──────────────────────────────────────────────
            var cat = new CourseCategories
            {
                Name = "Audit Category",
                Description = "Category for auditing tests"
            };
            DbContext.CourseCategories.Add(cat);
            await DbContext.SaveChangesAsync();

            var insertAudit = DbContext.AuditLogs.FirstOrDefault(a => a.TableName == "CourseCategories" && a.Action == ActionType.Insert);
            Assert.That(insertAudit, Is.Not.Null);
            Assert.That(insertAudit.RecordId, Is.EqualTo(cat.Id));
            Assert.That(insertAudit.NewValues, Contains.Substring("Audit Category"));

            // ─── 2. UPDATE AUDIT ──────────────────────────────────────────────
            cat.Description = "Updated description";
            DbContext.CourseCategories.Update(cat);
            await DbContext.SaveChangesAsync();

            var updateAudit = DbContext.AuditLogs.FirstOrDefault(a => a.TableName == "CourseCategories" && a.Action == ActionType.Update);
            Assert.That(updateAudit, Is.Not.Null);
            Assert.That(updateAudit.RecordId, Is.EqualTo(cat.Id));
            Assert.That(updateAudit.OldValues, Contains.Substring("Category for auditing tests"));
            Assert.That(updateAudit.NewValues, Contains.Substring("Updated description"));

            // ─── 3. DELETE AUDIT ──────────────────────────────────────────────
            DbContext.CourseCategories.Remove(cat);
            await DbContext.SaveChangesAsync();

            var deleteAudit = DbContext.AuditLogs.FirstOrDefault(a => a.TableName == "CourseCategories" && a.Action == ActionType.Delete);
            Assert.That(deleteAudit, Is.Not.Null);
            Assert.That(deleteAudit.RecordId, Is.EqualTo(cat.Id));
            Assert.That(deleteAudit.OldValues, Contains.Substring("Updated description"));
        }

        [Test]
        public async Task SaveChanges_TriggersAutomatedUserRegisterAndLoginActivities()
        {
            // ─── 1. USER REGISTER ACTIVITY ───────────────────────────────────
            var newUser = new Users
            {
                Email = "new_registrant@test.com",
                PasswordHash = "h",
                PasswordSalt = "s",
                RoleId = _testUser.RoleId
            };
            DbContext.Users.Add(newUser);
            await DbContext.SaveChangesAsync();

            var registerActivity = DbContext.ActivityLogs.FirstOrDefault(l => l.UserId == newUser.Id && l.ActivityType == ActivityType.UserRegister);
            Assert.That(registerActivity, Is.Not.Null);
            Assert.That(registerActivity.Description, Contains.Substring(newUser.Email));

            // ─── 2. USER LOGIN ACTIVITY (Triggers on LastLoginAt change) ──────
            newUser.LastLoginAt = DateTime.UtcNow;
            DbContext.Users.Update(newUser);
            await DbContext.SaveChangesAsync();

            var loginActivity = DbContext.ActivityLogs.FirstOrDefault(l => l.UserId == newUser.Id && l.ActivityType == ActivityType.UserLogin);
            Assert.That(loginActivity, Is.Not.Null);
            Assert.That(loginActivity.Description, Contains.Substring(newUser.Email));
        }

        [Test]
        public async Task SaveChanges_TriggersAutomatedCourseEnrollmentActivity()
        {
            var setup = await SetupCourseAndSection();

            // Create a valid CourseBatch to satisfy the FK constraint
            var batch = new CourseBatch
            {
                CourseId = setup.course.Id,
                Name = "Test Batch",
                MaxStudents = 50,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1)
            };
            DbContext.CourseBatches.Add(batch);
            await DbContext.SaveChangesAsync();

            var enrollment = new Enrollments
            {
                UserId = _testUser.Id,
                CourseId = setup.course.Id,
                EnrolledAt = DateTime.UtcNow,
                ProgressPercentage = 0,
                IsCompleted = false,
                EnrollmentStatus = EnrollmentStatus.Active,
                BatchId = batch.Id
            };
            DbContext.Enrollments.Add(enrollment);
            await DbContext.SaveChangesAsync();

            var activity = DbContext.ActivityLogs.FirstOrDefault(l => l.UserId == _testUser.Id && l.ActivityType == ActivityType.CourseEnrollment);
            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.Description, Contains.Substring($"Batch ID: {batch.Id}"));
        }

        [Test]
        public async Task GetActivityLogsAsync_RetrievesWithFiltersAndPagination()
        {
            // Seed multiple activity logs
            var l1 = new ActivityLogs { UserId = _testUser.Id, ActivityType = ActivityType.UserLogin, Description = "Login 1", Timestamp = DateTime.UtcNow.AddMinutes(-5) };
            var l2 = new ActivityLogs { UserId = _testUser.Id, ActivityType = ActivityType.CourseEnrollment, Description = "Enroll 1", Timestamp = DateTime.UtcNow.AddMinutes(-4) };
            var l3 = new ActivityLogs { UserId = _testUser.Id, ActivityType = ActivityType.UserLogin, Description = "Login 2", Timestamp = DateTime.UtcNow.AddMinutes(-3) };
            DbContext.ActivityLogs.AddRange(l1, l2, l3);
            await DbContext.SaveChangesAsync();

            // Filter by UserLogin
            var filterResult = (await _adminLogService.GetActivityLogsAsync(_testUser.Id, "UserLogin", 1, 10)).ToList();
            Assert.That(filterResult.Count, Is.EqualTo(2));
            Assert.That(filterResult[0].Description, Is.EqualTo("Login 2")); // Ordered desc by default
            Assert.That(filterResult[1].Description, Is.EqualTo("Login 1"));
            Assert.That(filterResult[0].UserEmail, Is.EqualTo(_testUser.Email));
        }

        [Test]
        public async Task GetAuditLogsAsync_RetrievesWithFiltersAndPagination()
        {
            // Seed audit logs
            var a1 = new AuditLogs { UserId = _testUser.Id, TableName = "Courses", RecordId = 10, Action = ActionType.Insert, Timestamp = DateTime.UtcNow.AddMinutes(-5) };
            var a2 = new AuditLogs { UserId = _testUser.Id, TableName = "Lessons", RecordId = 20, Action = ActionType.Update, Timestamp = DateTime.UtcNow.AddMinutes(-4) };
            var a3 = new AuditLogs { UserId = _testUser.Id, TableName = "Courses", RecordId = 10, Action = ActionType.Update, Timestamp = DateTime.UtcNow.AddMinutes(-3) };
            DbContext.AuditLogs.AddRange(a1, a2, a3);
            await DbContext.SaveChangesAsync();

            // Filter by Table
            var filterResult = (await _adminLogService.GetAuditLogsAsync(_testUser.Id, "Courses", null, 1, 10)).ToList();
            Assert.That(filterResult.Count, Is.EqualTo(2));
            Assert.That(filterResult[0].TableName, Is.EqualTo("Courses"));
            Assert.That(filterResult[0].Action, Is.EqualTo("Update"));
            Assert.That(filterResult[1].Action, Is.EqualTo("Insert"));
        }
    }
}
