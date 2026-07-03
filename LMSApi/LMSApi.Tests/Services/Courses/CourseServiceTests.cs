using System;
using System.Linq;
using System.Threading.Tasks;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Services;
using LMSApi.DALLibrary.Interfaces;
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
    public class CourseServiceTests : BaseServiceTest
    {
        private Mock<ILogger<CourseService>> _mockLogger = null!;
        private Mock<IUploadService> _mockUploadService = null!;
        private Mock<INotificationService> _mockNotificationService = null!;
        private Mock<IWishListRepository> _mockWishListRepository = null!;
        private Mock<IUserNotificationsService> _mockUserNotificationsService = null!;
        private ICourseService _courseService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mockLogger = new Mock<ILogger<CourseService>>();
            _mockUploadService = new Mock<IUploadService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockWishListRepository = new Mock<IWishListRepository>();
            _mockUserNotificationsService = new Mock<IUserNotificationsService>();
            
            var courseRepository = new CourseRepository(DbContext);
            var categoryRepository = new CourseCategoryRepository(DbContext);
            var userRepository = new UserRepository(DbContext);
            var enrollmentRepository = new EnrollmentRepository(DbContext);

            _courseService = new CourseService(
                courseRepository,
                categoryRepository,
                userRepository,
                enrollmentRepository,
                _mockUploadService.Object,
                Mapper,
                _mockLogger.Object,
                _mockNotificationService.Object,
                _mockWishListRepository.Object,
                _mockUserNotificationsService.Object,
                new Mock<IReviewRepository>().Object
            );
        }

        private async Task<(Users user, CourseCategories category)> CreateUserAndCategory()
        {
            var user = new Users { Email = "instr@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Instructor", Description = "Desc" } };
            var cat = new CourseCategories { Name = "Tech", Description = "Desc" };
            DbContext.Users.Add(user);
            DbContext.CourseCategories.Add(cat);
            await DbContext.SaveChangesAsync();
            return (user, cat);
        }

        private Courses BuildCourse(int userId, int catId, string slug = "test-slug", CourseStatus status = CourseStatus.Draft, CourseAccessType type = CourseAccessType.SelfPaced)
            => new Courses
            {
                Title = "Test", Description = "Desc", Price = 0m,
                ThumbnailUrl = "url", IntroVideoUrl = "url",
                IsPremium = false, Requirements = "R", LearningOutcomes = "LO",
                EstimatedDuration = TimeSpan.Zero,
                Level = CourseLevel.Beginner, LanguageId = 1,
                PublishedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
                DefaultDeadlineDays = 7, CategoryId = catId, InstructorId = userId,
                slug = slug, Status = status, CourseAccessType = type
            };

        // ─── CreateCourseAsync ─────────────────────────────────────────────────

        [Test]
        public async Task CreateCourseAsync_ValidRequest_CreatesCourse()
        {
            var (user, cat) = await CreateUserAndCategory();

            var request = new CreateCourseRequest
            {
                Title = "New Course",
                Description = "Desc",
                CategoryId = cat.Id,
                CourseAccessType = CourseAccessType.SelfPaced
            };

            var result = await _courseService.CreateCourseAsync(user.Id, request);

            Assert.That(result.Title, Is.EqualTo("New Course"));
            Assert.That(result.Status, Is.EqualTo(CourseStatus.Draft));
        }

        [Test]
        public async Task CreateCourseAsync_DuplicateTitle_ThrowsInvalidOperationException()
        {
            var (user, cat) = await CreateUserAndCategory();

            var course = BuildCourse(user.Id, cat.Id, "test-slug");
            course.Title = "Duplicate Title";
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            var request = new CreateCourseRequest
            {
                Title = "Duplicate Title",
                CategoryId = cat.Id
            };

            Assert.ThrowsAsync<InvalidOperationException>(() => _courseService.CreateCourseAsync(user.Id, request));
        }

        [Test]
        public void CreateCourseAsync_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _courseService.CreateCourseAsync(1, null!));
        }

        [Test]
        public async Task CreateCourseAsync_EmptyTitle_ThrowsArgumentException()
        {
            var (user, cat) = await CreateUserAndCategory();
            var request = new CreateCourseRequest { Title = "   ", CategoryId = cat.Id };
            Assert.ThrowsAsync<ArgumentException>(() => _courseService.CreateCourseAsync(user.Id, request));
        }

        [Test]
        public async Task CreateCourseAsync_IsPremiumWithNoPrice_ThrowsArgumentException()
        {
            var (user, cat) = await CreateUserAndCategory();
            var request = new CreateCourseRequest
            {
                Title = "Premium Course",
                CategoryId = cat.Id,
                IsPremium = true,
                Price = null // no price
            };
            Assert.ThrowsAsync<ArgumentException>(() => _courseService.CreateCourseAsync(user.Id, request));
        }

        [Test]
        public async Task CreateCourseAsync_IsPremiumWithZeroPrice_ThrowsArgumentException()
        {
            var (user, cat) = await CreateUserAndCategory();
            var request = new CreateCourseRequest
            {
                Title = "Premium Course",
                CategoryId = cat.Id,
                IsPremium = true,
                Price = 0m // zero price is invalid
            };
            Assert.ThrowsAsync<ArgumentException>(() => _courseService.CreateCourseAsync(user.Id, request));
        }

        // ─── GetAllCoursesAsync ────────────────────────────────────────────────

        [Test]
        public async Task GetAllCoursesAsync_NoPublishedCourses_ReturnsEmptyList()
        {
            var result = await _courseService.GetAllCoursesAsync();
            Assert.That(result, Is.Empty);
        }

        // ─── GetCourseByIdAsync ────────────────────────────────────────────────

        [Test]
        public void GetCourseByIdAsync_NotFound_ThrowsKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _courseService.GetCourseByIdAsync(9999));
        }

        // ─── UpdateCourseAsync ─────────────────────────────────────────────────

        [Test]
        public async Task UpdateCourseAsync_ValidUpdate_UpdatesCourse()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = BuildCourse(user.Id, cat.Id, "old-slug");
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            var request = new UpdateCourseRequest { Title = "Updated", Description = "Desc" };

            var result = await _courseService.UpdateCourseAsync(course.Id, request);

            Assert.That(result.Title, Is.EqualTo("Updated"));
            Assert.That(result.Description, Is.EqualTo("Desc"));
        }

        [Test]
        public async Task UpdateCourseAsync_DuplicateTitle_ThrowsInvalidOperationException()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course1 = BuildCourse(user.Id, cat.Id, "slug-1");
            course1.Title = "Course One";
            var course2 = BuildCourse(user.Id, cat.Id, "slug-2");
            course2.Title = "Course Two";
            DbContext.Courses.Add(course1);
            DbContext.Courses.Add(course2);
            await DbContext.SaveChangesAsync();

            var request = new UpdateCourseRequest { Title = "Course Two" };

            Assert.ThrowsAsync<InvalidOperationException>(() => _courseService.UpdateCourseAsync(course1.Id, request));
        }

        [Test]
        public void UpdateCourseAsync_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _courseService.UpdateCourseAsync(1, null!));
        }

        [Test]
        public async Task UpdateCourseAsync_IsPremiumTrueWithZeroPrice_ThrowsArgumentException()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = BuildCourse(user.Id, cat.Id, "slug-premium");
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            var request = new UpdateCourseRequest { IsPremium = true, Price = 0m };
            Assert.ThrowsAsync<ArgumentException>(() => _courseService.UpdateCourseAsync(course.Id, request));
        }

        [Test]
        public void UpdateCourseAsync_CourseNotFound_ThrowsKeyNotFoundException()
        {
            var request = new UpdateCourseRequest { Title = "X" };
            Assert.ThrowsAsync<KeyNotFoundException>(() => _courseService.UpdateCourseAsync(99999, request));
        }

        // ─── DeleteCourseAsync ─────────────────────────────────────────────────

        [Test]
        public async Task DeleteCourseAsync_NoEnrollments_DeletesSuccessfully()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = BuildCourse(user.Id, cat.Id, "delete-me");
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            await _courseService.DeleteCourseAsync(course.Id);

            var dbCourse = await DbContext.Courses.FindAsync(course.Id);
            Assert.That(dbCourse, Is.Null);
        }

        [Test]
        public void DeleteCourseAsync_NotFound_ThrowsKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _courseService.DeleteCourseAsync(88888));
        }

        [Test]
        public async Task DeleteCourseAsync_WithEnrollments_ThrowsException()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = BuildCourse(user.Id, cat.Id, "with-enrollments");
            DbContext.Courses.Add(course);
            var student = new Users { Email = "student@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Learner", Description = "Desc" } };
            DbContext.Users.Add(student);
            await DbContext.SaveChangesAsync();

            DbContext.Enrollments.Add(new Enrollments
            {
                CourseId = course.Id,
                UserId = student.Id,
                EnrolledAt = DateTime.UtcNow,
                ProgressPercentage = 0,
                IsCompleted = false
            });
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() => _courseService.DeleteCourseAsync(course.Id));
        }

        // ─── PublishCourseAsync ────────────────────────────────────────────────

        [Test]
        public async Task PublishCourseAsync_SelfPaced_TransitionsToPendingApprovalAndKeepsSectionsDraft()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = BuildCourse(user.Id, cat.Id, "to-publish", CourseStatus.Draft, CourseAccessType.SelfPaced);
            var section = new CourseSection { Title = "S1", Description = "Desc", SectionId = 1, Status = PublishStatus.Draft };
            section.Lessons.Add(new Lessons { Title = "L1", Type = LessonType.Video });
            course.Sections.Add(section);
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            var result = await _courseService.PublishCourseAsync(course.Id, new PublishCourseRequest { Publish = true });

            Assert.That(result.Status, Is.EqualTo(CourseStatus.PendingApproval));
            var dbSection = await DbContext.CourseSections.FindAsync(section.Id);
            Assert.That(dbSection!.Status, Is.EqualTo(PublishStatus.Draft));
        }

        [Test]
        public async Task PublishCourseAsync_NoSections_ThrowsInvalidOperationException()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = BuildCourse(user.Id, cat.Id, "no-sections", CourseStatus.Draft);
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _courseService.PublishCourseAsync(course.Id, new PublishCourseRequest { Publish = true }));
        }

        [Test]
        public async Task PublishCourseAsync_SectionWithNoLessons_ThrowsInvalidOperationException()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = BuildCourse(user.Id, cat.Id, "no-lessons", CourseStatus.Draft);
            course.Sections.Add(new CourseSection { Title = "S1", Description = "Desc", SectionId = 1 });
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _courseService.PublishCourseAsync(course.Id, new PublishCourseRequest { Publish = true }));
        }

        [Test]
        public async Task PublishCourseAsync_QuizWithNoQuestions_ThrowsInvalidOperationException()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = BuildCourse(user.Id, cat.Id, "no-questions", CourseStatus.Draft);
            var section = new CourseSection { Title = "S1", Description = "Desc", SectionId = 1 };
            section.Lessons.Add(new Lessons { Title = "L1", Type = LessonType.Video });
            section.Quizzes.Add(new Quzzes { Title = "Q1" });
            course.Sections.Add(section);
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _courseService.PublishCourseAsync(course.Id, new PublishCourseRequest { Publish = true }));
        }

        [Test]
        public async Task PublishCourseAsync_AssignmentWithZeroMarks_ThrowsInvalidOperationException()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = BuildCourse(user.Id, cat.Id, "zero-marks", CourseStatus.Draft);
            var section = new CourseSection { Title = "S1", Description = "Desc", SectionId = 1 };
            section.Lessons.Add(new Lessons { Title = "L1", Type = LessonType.Video });
            section.Assignments.Add(new Assignments { Title = "A1", TotalMarks = 0 });
            course.Sections.Add(section);
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _courseService.PublishCourseAsync(course.Id, new PublishCourseRequest { Publish = true }));
        }

        [Test]
        public async Task PublishCourseAsync_AlreadyPublished_ThrowsInvalidOperationException()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = BuildCourse(user.Id, cat.Id, "already-pub", CourseStatus.Published);
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _courseService.PublishCourseAsync(course.Id, new PublishCourseRequest { Publish = true }));
        }

        [Test]
        public async Task PublishCourseAsync_AlreadyPendingApproval_ThrowsInvalidOperationException()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = BuildCourse(user.Id, cat.Id, "already-pending", CourseStatus.PendingApproval);
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _courseService.PublishCourseAsync(course.Id, new PublishCourseRequest { Publish = true }));
        }

        [Test]
        public async Task PublishCourseAsync_CohortBasedWithoutBatches_ThrowsException()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = BuildCourse(user.Id, cat.Id, "cohort", CourseStatus.Draft, CourseAccessType.CohortBased);
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() => _courseService.PublishCourseAsync(course.Id, new PublishCourseRequest { Publish = true }));
        }

        [Test]
        public async Task PublishCourseAsync_UnpublishAlreadyDraft_ThrowsInvalidOperationException()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = BuildCourse(user.Id, cat.Id, "already-draft", CourseStatus.Draft);
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _courseService.PublishCourseAsync(course.Id, new PublishCourseRequest { Publish = false }));
        }

        [Test]
        public async Task PublishCourseAsync_UnpublishFromPublishedOrPendingApproval_TransitionsToDraft()
        {
            var (user, cat) = await CreateUserAndCategory();
            
            var course1 = BuildCourse(user.Id, cat.Id, "pub-course", CourseStatus.Published);
            DbContext.Courses.Add(course1);
            
            var course2 = BuildCourse(user.Id, cat.Id, "pending-course", CourseStatus.PendingApproval);
            DbContext.Courses.Add(course2);
            
            await DbContext.SaveChangesAsync();

            var result1 = await _courseService.PublishCourseAsync(course1.Id, new PublishCourseRequest { Publish = false });
            Assert.That(result1.Status, Is.EqualTo(CourseStatus.Draft));

            await Task.Delay(100);
            _mockNotificationService.Verify(x => x.Send(It.IsAny<Message>()), Times.Once);

            var result2 = await _courseService.PublishCourseAsync(course2.Id, new PublishCourseRequest { Publish = false });
            Assert.That(result2.Status, Is.EqualTo(CourseStatus.Draft));
        }

        // ─── ReviewCourseAsync ─────────────────────────────────────────────────

        [Test]
        public async Task ReviewCourseAsync_Approve_PendingApprovalCourse_PublishesCourseAndContent()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = BuildCourse(user.Id, cat.Id, "to-approve", CourseStatus.PendingApproval, CourseAccessType.SelfPaced);
            var section = new CourseSection { Title = "S1", Description = "Desc", SectionId = 1, Status = PublishStatus.Draft };
            course.Sections.Add(section);
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            var req = new ReviewCourseRequest { Action = "Approve" };
            var result = await _courseService.ReviewCourseAsync(course.Id, req);

            Assert.That(result.Status, Is.EqualTo(CourseStatus.Published));
            Assert.That(result.PublishedAt, Is.Not.Null);
            
            var dbSection = await DbContext.CourseSections.FindAsync(section.Id);
            Assert.That(dbSection!.Status, Is.EqualTo(PublishStatus.Published));

            await Task.Delay(100);
            _mockNotificationService.Verify(x => x.Send(It.IsAny<Message>()), Times.Once);
        }

        [Test]
        public async Task ReviewCourseAsync_Reject_PendingApprovalCourse_SetsStatusToRejectedAndSendsEmail()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = BuildCourse(user.Id, cat.Id, "to-reject", CourseStatus.PendingApproval);
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            var req = new ReviewCourseRequest { Action = "Reject", Reason = "Incomplete content" };
            var result = await _courseService.ReviewCourseAsync(course.Id, req);

            Assert.That(result.Status, Is.EqualTo(CourseStatus.Rejected));

            await Task.Delay(100);
            _mockNotificationService.Verify(x => x.Send(It.IsAny<Message>()), Times.Once);
        }

        [Test]
        public async Task ReviewCourseAsync_Reject_WithoutReason_ThrowsArgumentException()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = BuildCourse(user.Id, cat.Id, "to-reject-no-reason", CourseStatus.PendingApproval);
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            var req = new ReviewCourseRequest { Action = "Reject", Reason = "" };
            Assert.ThrowsAsync<ArgumentException>(() => _courseService.ReviewCourseAsync(course.Id, req));
        }

        [Test]
        public async Task ReviewCourseAsync_InvalidAction_ThrowsArgumentException()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = BuildCourse(user.Id, cat.Id, "to-review-invalid-action", CourseStatus.PendingApproval);
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            var req = new ReviewCourseRequest { Action = "InvalidAction" };
            Assert.ThrowsAsync<ArgumentException>(() => _courseService.ReviewCourseAsync(course.Id, req));
        }

        [Test]
        public void ReviewCourseAsync_NotFound_ThrowsKeyNotFoundException()
        {
            var req = new ReviewCourseRequest { Action = "Approve" };
            Assert.ThrowsAsync<KeyNotFoundException>(() => _courseService.ReviewCourseAsync(99999, req));
        }

        [Test]
        public async Task ReviewCourseAsync_NotPendingApproval_ThrowsInvalidOperationException()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = BuildCourse(user.Id, cat.Id, "not-pending", CourseStatus.Draft);
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            var req = new ReviewCourseRequest { Action = "Approve" };
            Assert.ThrowsAsync<InvalidOperationException>(() => _courseService.ReviewCourseAsync(course.Id, req));
        }

        // ─── GetPendingCoursesAsync ────────────────────────────────────────────

        [Test]
        public async Task GetPendingCoursesAsync_ReturnsOnlyPendingCourses()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course1 = BuildCourse(user.Id, cat.Id, "course1", CourseStatus.PendingApproval);
            var course2 = BuildCourse(user.Id, cat.Id, "course2", CourseStatus.Draft);
            var course3 = BuildCourse(user.Id, cat.Id, "course3", CourseStatus.Published);
            DbContext.Courses.AddRange(course1, course2, course3);
            await DbContext.SaveChangesAsync();

            var result = await _courseService.GetPendingCoursesAsync();

            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Id, Is.EqualTo(course1.Id));
        }

        // ─── GetCoursesByInstructorAsync / GetCoursesByCategoryAsync ──────────

        [Test]
        public async Task GetCoursesByInstructorAsync_NoCoursesForInstructor_ReturnsEmpty()
        {
            var result = await _courseService.GetCoursesByInstructorAsync(9999);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetCoursesByCategoryAsync_NoCoursesForCategory_ReturnsEmpty()
        {
            var result = await _courseService.GetCoursesByCategoryAsync(9999);
            Assert.That(result, Is.Empty);
        }
    }
}
