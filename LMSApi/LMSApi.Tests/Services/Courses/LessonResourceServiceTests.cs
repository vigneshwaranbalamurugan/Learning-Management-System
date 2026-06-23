using System;
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
    public class LessonResourceServiceTests : BaseServiceTest
    {
        private Mock<ILogger<LessonResourceService>> _mockLogger = null!;
        private Mock<IUploadService> _mockUploadService = null!;
        private Mock<INotificationService> _mockNotificationService = null!;
        private Mock<IUserNotificationsService> _mockUserNotificationsService = null!;
        private ILessonResourceService _resourceService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mockLogger = new Mock<ILogger<LessonResourceService>>();
            _mockUploadService = new Mock<IUploadService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockUserNotificationsService = new Mock<IUserNotificationsService>();
            
            var resourceRepository = new LessonResourceRepository(DbContext);
            var lessonRepository = new LessonRepository(DbContext);
            var sectionRepository = new CourseSectionRepository(DbContext);
            var courseRepository = new CourseRepository(DbContext);
            var enrollmentRepository = new EnrollmentRepository(DbContext);

            _resourceService = new LessonResourceService(
                resourceRepository,
                lessonRepository,
                sectionRepository,
                courseRepository,
                _mockUploadService.Object,
                Mapper,
                _mockLogger.Object,
                enrollmentRepository,
                _mockNotificationService.Object,
                _mockUserNotificationsService.Object
            );
        }

        private async Task<(Lessons lesson, Courses course)> SetupLessonWithCourse(
            CourseAccessType type = CourseAccessType.SelfPaced,
            CourseStatus status = CourseStatus.Draft)
        {
            var inst = new Users { Email = $"{Guid.NewGuid()}@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Instructor", Description = "Desc" } };
            var cat = new CourseCategories { Name = "Cat", Description = "Desc" };
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

            var section = new CourseSection { Title = "Sec", Description = "Desc", SectionId = 1, CourseId = course.Id };
            DbContext.CourseSections.Add(section);
            await DbContext.SaveChangesAsync();

            var lesson = new Lessons { Title = "Lesson", Description = "Desc", Content = "Content", ContentUrl = "url", CourseSectionId = section.Id };
            DbContext.Lessons.Add(lesson);
            await DbContext.SaveChangesAsync();

            return (lesson, course);
        }

        private LessonResources BuildResource(int lessonId)
            => new LessonResources
            {
                LessonId = lessonId,
                ResourceUrl = "url",
                ResourceTitle = "Title",
                Description = "Desc",
                ResourceType = ResourceType.Pdf
            };

        // ─── DeleteResourceAsync ───────────────────────────────────────────────

        [Test]
        public async Task DeleteResourceAsync_DeletesResource()
        {
            var (lesson, _) = await SetupLessonWithCourse();
            var resource = BuildResource(lesson.Id);
            DbContext.LessonResources.Add(resource);
            await DbContext.SaveChangesAsync();

            await _resourceService.DeleteResourceAsync(resource.Id);

            var dbRes = await DbContext.LessonResources.FindAsync(resource.Id);
            Assert.That(dbRes, Is.Null);
        }

        [Test]
        public void DeleteResourceAsync_NotFound_ThrowsKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _resourceService.DeleteResourceAsync(99999));
        }

        // ─── AddResourceAsync ──────────────────────────────────────────────────

        [Test]
        public void AddResourceAsync_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _resourceService.AddResourceAsync(null!));
        }

        [Test]
        public async Task AddResourceAsync_ExternalLinkWithoutUrl_ThrowsArgumentException()
        {
            var (lesson, _) = await SetupLessonWithCourse();
            var request = new CreateResourceRequest
            {
                LessonId = lesson.Id,
                ResourceTitle = "Ext",
                ResourceType = ResourceType.ExternalLink,
                ResourceUrl = "" // missing
            };
            Assert.ThrowsAsync<ArgumentException>(() => _resourceService.AddResourceAsync(request));
        }

        [Test]
        public async Task AddResourceAsync_ExternalLinkWithFileStream_ThrowsArgumentException()
        {
            var (lesson, _) = await SetupLessonWithCourse();
            var request = new CreateResourceRequest
            {
                LessonId = lesson.Id,
                ResourceTitle = "Ext",
                ResourceType = ResourceType.ExternalLink,
                ResourceUrl = "https://example.com"
            };
            using var fakeStream = new System.IO.MemoryStream(new byte[] { 1, 2, 3 });
            // providing a file stream for an external link is invalid
            Assert.ThrowsAsync<ArgumentException>(() => _resourceService.AddResourceAsync(request, fakeStream, "file.pdf"));
        }

        [Test]
        public async Task AddResourceAsync_PdfWithoutFileStream_ThrowsArgumentException()
        {
            var (lesson, _) = await SetupLessonWithCourse();
            var request = new CreateResourceRequest
            {
                LessonId = lesson.Id,
                ResourceTitle = "PDF",
                ResourceType = ResourceType.Pdf
                // no fileStream
            };
            Assert.ThrowsAsync<ArgumentException>(() => _resourceService.AddResourceAsync(request));
        }

        [Test]
        public async Task AddResourceAsync_ExternalLink_ValidRequest_AddsResource()
        {
            var (lesson, _) = await SetupLessonWithCourse();
            var request = new CreateResourceRequest
            {
                LessonId = lesson.Id,
                ResourceTitle = "Link",
                Description = string.Empty,
                ResourceType = ResourceType.ExternalLink,
                ResourceUrl = "https://example.com"
            };

            var result = await _resourceService.AddResourceAsync(request);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ResourceUrl, Is.EqualTo("https://example.com"));
        }

        // ─── PublishResourceAsync ──────────────────────────────────────────────

        [Test]
        public async Task PublishResourceAsync_SelfPacedCourse_ThrowsInvalidOperationException()
        {
            var (lesson, _) = await SetupLessonWithCourse(CourseAccessType.SelfPaced);
            var resource = BuildResource(lesson.Id);
            DbContext.LessonResources.Add(resource);
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _resourceService.PublishResourceAsync(resource.Id, new PublishResourceRequest { Publish = true }));
        }

        [Test]
        public async Task PublishResourceAsync_CohortBased_PublishesResource()
        {
            var (lesson, _) = await SetupLessonWithCourse(CourseAccessType.CohortBased);
            var resource = BuildResource(lesson.Id);
            resource.Status = PublishStatus.Draft;
            DbContext.LessonResources.Add(resource);
            await DbContext.SaveChangesAsync();

            var result = await _resourceService.PublishResourceAsync(resource.Id, new PublishResourceRequest { Publish = true });
            Assert.That(result.Status, Is.EqualTo(PublishStatus.Published));
        }

        [Test]
        public async Task PublishResourceAsync_CohortBased_WithEnrolledLearners_SendsNotificationEmails()
        {
            var (lesson, course) = await SetupLessonWithCourse(CourseAccessType.CohortBased);
            var resource = BuildResource(lesson.Id);
            resource.Status = PublishStatus.Draft;
            DbContext.LessonResources.Add(resource);

            var student = new Users { Email = "rstudent@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Learner", Description = "Desc" } };
            DbContext.Users.Add(student);
            await DbContext.SaveChangesAsync();

            DbContext.Enrollments.Add(new Enrollments
            {
                CourseId = course.Id, UserId = student.Id,
                EnrollmentStatus = EnrollmentStatus.Active, EnrolledAt = DateTime.UtcNow,
                ProgressPercentage = 0, IsCompleted = false
            });
            await DbContext.SaveChangesAsync();

            await _resourceService.PublishResourceAsync(resource.Id, new PublishResourceRequest { Publish = true });
            
            await Task.Delay(100);
            _mockNotificationService.Verify(x => x.Send(It.IsAny<Message>()), Times.Once);
        }

        [Test]
        public async Task PublishResourceAsync_CohortBased_NoEnrolledLearners_SendsNoEmail()
        {
            var (lesson, _) = await SetupLessonWithCourse(CourseAccessType.CohortBased);
            var resource = BuildResource(lesson.Id);
            resource.Status = PublishStatus.Draft;
            DbContext.LessonResources.Add(resource);
            await DbContext.SaveChangesAsync();

            await _resourceService.PublishResourceAsync(resource.Id, new PublishResourceRequest { Publish = true });
            
            await Task.Delay(100);
            _mockNotificationService.Verify(x => x.Send(It.IsAny<Message>()), Times.Never);
        }

        // ─── UpdateResourceAsync ───────────────────────────────────────────────

        [Test]
        public async Task UpdateResourceAsync_ManualStatusChangeSelfPaced_ThrowsInvalidOperationException()
        {
            var (lesson, _) = await SetupLessonWithCourse(CourseAccessType.SelfPaced);
            var resource = BuildResource(lesson.Id);
            DbContext.LessonResources.Add(resource);
            await DbContext.SaveChangesAsync();

            var request = new UpdateResourceRequest { Status = PublishStatus.Published };
            Assert.ThrowsAsync<InvalidOperationException>(() => _resourceService.UpdateResourceAsync(resource.Id, request));
        }
    }
}
