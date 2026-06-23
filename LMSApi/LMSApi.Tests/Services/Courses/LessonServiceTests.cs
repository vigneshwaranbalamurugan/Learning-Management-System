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
    public class LessonServiceTests : BaseServiceTest
    {
        private Mock<ILogger<LessonService>> _mockLogger = null!;
        private Mock<IUploadService> _mockUploadService = null!;
        private Mock<INotificationService> _mockNotificationService = null!;
        private ILessonService _lessonService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mockLogger = new Mock<ILogger<LessonService>>();
            _mockUploadService = new Mock<IUploadService>();
            _mockNotificationService = new Mock<INotificationService>();
            
            var lessonRepository = new LessonRepository(DbContext);
            var sectionRepository = new CourseSectionRepository(DbContext);
            var courseRepository = new CourseRepository(DbContext);
            var enrollmentRepository = new EnrollmentRepository(DbContext);
            var mockUserNotificationsService = new Mock<IUserNotificationsService>();

            _lessonService = new LessonService(
                lessonRepository,
                sectionRepository,
                courseRepository,
                _mockUploadService.Object,
                Mapper,
                _mockLogger.Object,
                enrollmentRepository,
                _mockNotificationService.Object,
                mockUserNotificationsService.Object
            );
        }

        private async Task<CourseSection> SetupSection(
            CourseAccessType type = CourseAccessType.SelfPaced,
            CourseStatus status = CourseStatus.Draft)
        {
            var cat = new CourseCategories { Name = "Cat", Description = "Desc" };
            var inst = new Users { Email = $"{Guid.NewGuid()}@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Instructor", Description = "Desc" } };
            DbContext.Users.Add(inst);
            DbContext.CourseCategories.Add(cat);
            await DbContext.SaveChangesAsync();

            var course = new Courses
            {
                Title = "Course", Description = "Desc", Price = 0m, ThumbnailUrl = "url", IntroVideoUrl = "url",
                IsPremium = false, Requirements = "Reqs", LearningOutcomes = "Outcomes",
                EstimatedDuration = TimeSpan.Zero, Level = CourseLevel.Beginner, LanguageId = 1,
                PublishedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
                DefaultDeadlineDays = 7, CategoryId = cat.Id, InstructorId = inst.Id,
                slug = Guid.NewGuid().ToString(),
                CourseAccessType = type, Status = status
            };
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            var section = new CourseSection { Title = "Section 1", Description = "Desc", SectionId = 1, CourseId = course.Id };
            DbContext.CourseSections.Add(section);
            await DbContext.SaveChangesAsync();

            return section;
        }

        // ─── CreateLessonAsync ─────────────────────────────────────────────────

        [Test]
        public async Task CreateLessonAsync_ValidRequest_CreatesLesson()
        {
            var section = await SetupSection();
            var request = new CreateLessonRequest { Title = "Lesson 1", CourseSectionId = section.Id };

            var result = await _lessonService.CreateLessonAsync(request);

            Assert.That(result.Title, Is.EqualTo("Lesson 1"));
            Assert.That(result.CourseSectionId, Is.EqualTo(section.Id));
            Assert.That(result.SortOrder, Is.EqualTo(1));
        }

        [Test]
        public void CreateLessonAsync_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _lessonService.CreateLessonAsync(null!));
        }

        [Test]
        public async Task CreateLessonAsync_EmptyTitle_ThrowsArgumentException()
        {
            var section = await SetupSection();
            var request = new CreateLessonRequest { Title = "  ", CourseSectionId = section.Id };
            Assert.ThrowsAsync<ArgumentException>(() => _lessonService.CreateLessonAsync(request));
        }

        [Test]
        public async Task CreateLessonAsync_SelfPacedPublishedCourse_LessonIsAutoPublished()
        {
            var section = await SetupSection(CourseAccessType.SelfPaced, CourseStatus.Published);
            var request = new CreateLessonRequest { Title = "Auto Published Lesson", CourseSectionId = section.Id };

            var result = await _lessonService.CreateLessonAsync(request);

            Assert.That(result.Status, Is.EqualTo(PublishStatus.Published));
        }

        [Test]
        public async Task CreateLessonAsync_SelfPacedDraftCourse_LessonIsNotAutoPublished()
        {
            var section = await SetupSection(CourseAccessType.SelfPaced, CourseStatus.Draft);
            var request = new CreateLessonRequest { Title = "Draft Lesson", CourseSectionId = section.Id };

            var result = await _lessonService.CreateLessonAsync(request);

            Assert.That(result.Status, Is.EqualTo(PublishStatus.Draft));
        }

        // ─── UpdateLessonAsync ─────────────────────────────────────────────────

        [Test]
        public async Task UpdateLessonAsync_ValidRequest_UpdatesLesson()
        {
            var section = await SetupSection();
            var lesson = new Lessons { Title = "Old", Description = "Desc", Content = "Content", ContentUrl = "url", CourseSectionId = section.Id };
            DbContext.Lessons.Add(lesson);
            await DbContext.SaveChangesAsync();

            var request = new UpdateLessonRequest { Title = "New" };
            var result = await _lessonService.UpdateLessonAsync(lesson.Id, request);

            Assert.That(result.Title, Is.EqualTo("New"));
        }

        [Test]
        public async Task UpdateLessonAsync_ManualStatusChangeSelfPaced_ThrowsInvalidOperationException()
        {
            var section = await SetupSection(CourseAccessType.SelfPaced);
            var lesson = new Lessons { Title = "Lesson", Description = "Desc", Content = "C", ContentUrl = "url", CourseSectionId = section.Id };
            DbContext.Lessons.Add(lesson);
            await DbContext.SaveChangesAsync();

            var request = new UpdateLessonRequest { Status = PublishStatus.Published };
            Assert.ThrowsAsync<InvalidOperationException>(() => _lessonService.UpdateLessonAsync(lesson.Id, request));
        }

        // ─── DeleteLessonAsync ─────────────────────────────────────────────────

        [Test]
        public async Task DeleteLessonAsync_DeletesLesson()
        {
            var section = await SetupSection();
            var lesson = new Lessons { Title = "To Delete", Description = "Desc", Content = "Content", ContentUrl = "url", CourseSectionId = section.Id };
            DbContext.Lessons.Add(lesson);
            await DbContext.SaveChangesAsync();

            await _lessonService.DeleteLessonAsync(lesson.Id);

            var dbLesson = await DbContext.Lessons.FindAsync(lesson.Id);
            Assert.That(dbLesson, Is.Null);
        }

        [Test]
        public void DeleteLessonAsync_NotFound_ThrowsKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _lessonService.DeleteLessonAsync(99999));
        }

        // ─── PublishLessonAsync ────────────────────────────────────────────────

        [Test]
        public async Task PublishLessonAsync_SelfPacedCourse_ThrowsInvalidOperationException()
        {
            var section = await SetupSection(CourseAccessType.SelfPaced);
            var lesson = new Lessons { Title = "L", Description = "D", Content = "C", ContentUrl = "url", CourseSectionId = section.Id };
            DbContext.Lessons.Add(lesson);
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _lessonService.PublishLessonAsync(lesson.Id, new PublishLessonRequest { Publish = true }));
        }

        [Test]
        public async Task PublishLessonAsync_CohortBased_PublishesLesson()
        {
            var section = await SetupSection(CourseAccessType.CohortBased);
            var lesson = new Lessons { Title = "L", Description = "D", Content = "C", ContentUrl = "url", CourseSectionId = section.Id, Status = PublishStatus.Draft };
            DbContext.Lessons.Add(lesson);
            await DbContext.SaveChangesAsync();

            var result = await _lessonService.PublishLessonAsync(lesson.Id, new PublishLessonRequest { Publish = true });
            Assert.That(result.Status, Is.EqualTo(PublishStatus.Published));
        }

        [Test]
        public async Task PublishLessonAsync_CohortBased_WithEnrolledLearners_SendsNotificationEmails()
        {
            var section = await SetupSection(CourseAccessType.CohortBased);
            var lesson = new Lessons { Title = "L", Description = "D", Content = "C", ContentUrl = "url", CourseSectionId = section.Id, Status = PublishStatus.Draft };
            DbContext.Lessons.Add(lesson);

            var student = new Users { Email = "lstudent@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Learner", Description = "Desc" } };
            DbContext.Users.Add(student);
            await DbContext.SaveChangesAsync();

            DbContext.Enrollments.Add(new Enrollments
            {
                CourseId = section.CourseId, UserId = student.Id,
                EnrollmentStatus = EnrollmentStatus.Active, EnrolledAt = DateTime.UtcNow,
                ProgressPercentage = 0, IsCompleted = false
            });
            await DbContext.SaveChangesAsync();

            await _lessonService.PublishLessonAsync(lesson.Id, new PublishLessonRequest { Publish = true });
            
            await Task.Delay(100);
            _mockNotificationService.Verify(x => x.Send(It.IsAny<Message>()), Times.Once);
        }

        [Test]
        public async Task PublishLessonAsync_CohortBased_NoEnrolledLearners_SendsNoEmail()
        {
            var section = await SetupSection(CourseAccessType.CohortBased);
            var lesson = new Lessons { Title = "L", Description = "D", Content = "C", ContentUrl = "url", CourseSectionId = section.Id, Status = PublishStatus.Draft };
            DbContext.Lessons.Add(lesson);
            await DbContext.SaveChangesAsync();

            await _lessonService.PublishLessonAsync(lesson.Id, new PublishLessonRequest { Publish = true });
            
            await Task.Delay(100);
            _mockNotificationService.Verify(x => x.Send(It.IsAny<Message>()), Times.Never);
        }

        // ─── GetLessonsBySectionAsync ──────────────────────────────────────────

        [Test]
        public async Task GetLessonsBySectionAsync_NonInstructorSeesOnlyPublishedLessons()
        {
            var section = await SetupSection(CourseAccessType.CohortBased);
            var pubLesson = new Lessons { Title = "Pub", Description = "D", Content = "C", ContentUrl = "u", CourseSectionId = section.Id, Status = PublishStatus.Published };
            var draftLesson = new Lessons { Title = "Draft", Description = "D", Content = "C", ContentUrl = "u", CourseSectionId = section.Id, Status = PublishStatus.Draft };
            DbContext.Lessons.AddRange(pubLesson, draftLesson);
            await DbContext.SaveChangesAsync();

            var result = (await _lessonService.GetLessonsBySectionAsync(section.Id, currentUserId: null)).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("Pub"));
        }
    }
}
