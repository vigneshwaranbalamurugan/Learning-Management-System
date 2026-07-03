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
    public class CourseSectionServiceTests : BaseServiceTest
    {
        private Mock<ILogger<CourseSectionService>> _mockLogger = null!;
        private ICourseSectionService _sectionService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mockLogger = new Mock<ILogger<CourseSectionService>>();
            
            var sectionRepository = new CourseSectionRepository(DbContext);
            var courseRepository = new CourseRepository(DbContext);
            var enrollmentRepository = new EnrollmentRepository(DbContext);

            _sectionService = new CourseSectionService(
                sectionRepository,
                courseRepository,
                enrollmentRepository,
                Mapper,
                _mockLogger.Object
            );
        }

        private async Task<Courses> CreateTestCourse(
            CourseAccessType accessType = CourseAccessType.SelfPaced,
            CourseStatus status = CourseStatus.Draft)
        {
            var cat = new CourseCategories { Name = "Cat", Description = "Desc" };
            var user = new Users { Email = $"{Guid.NewGuid()}@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Instructor", Description = "Desc" } };
            DbContext.CourseCategories.Add(cat);
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

            var course = new Courses 
            { 
                Title = "Test Course", Description = "Desc", Price = 0m,
                ThumbnailUrl = "url", IntroVideoUrl = "url", IsPremium = false,
                Requirements = "Reqs", LearningOutcomes = "Outcomes",
                EstimatedDuration = TimeSpan.Zero,
                Level = CourseLevel.Beginner, LanguageId = 1,
                PublishedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
                DefaultDeadlineDays = 7, CategoryId = cat.Id, InstructorId = user.Id,
                slug = Guid.NewGuid().ToString(),
                CourseAccessType = accessType, Status = status
            };
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();
            return course;
        }

        // ─── CreateSectionAsync ────────────────────────────────────────────────

        [Test]
        public async Task CreateSectionAsync_ValidRequest_CreatesSection()
        {
            var course = await CreateTestCourse();
            var request = new CreateSectionRequest { Title = "Section 1", CourseId = course.Id, Description = "Desc" };

            var result = await _sectionService.CreateSectionAsync(request);

            Assert.That(result.Title, Is.EqualTo("Section 1"));
            Assert.That(result.CourseId, Is.EqualTo(course.Id));
            Assert.That(result.SortOrder, Is.EqualTo(1));
        }

        [Test]
        public void CreateSectionAsync_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _sectionService.CreateSectionAsync(null!));
        }

        [Test]
        public async Task CreateSectionAsync_EmptyTitle_ThrowsArgumentException()
        {
            var course = await CreateTestCourse();
            var request = new CreateSectionRequest { Title = "   ", CourseId = course.Id };
            Assert.ThrowsAsync<ArgumentException>(() => _sectionService.CreateSectionAsync(request));
        }

        [Test]
        public async Task CreateSectionAsync_SelfPacedPublishedCourse_SectionIsAutoPublished()
        {
            // When the course is SelfPaced AND Published, new sections are auto-published
            var course = await CreateTestCourse(CourseAccessType.SelfPaced, CourseStatus.Published);
            var request = new CreateSectionRequest { Title = "Auto Published", CourseId = course.Id, Description = "Desc" };

            var result = await _sectionService.CreateSectionAsync(request);

            Assert.That(result.Status, Is.EqualTo(PublishStatus.Published));
        }

        [Test]
        public async Task CreateSectionAsync_SecondSection_GetsIncrementedSortOrder()
        {
            var course = await CreateTestCourse();
            var section1 = new CourseSection { Title = "S1", Description = "Desc", SectionId = 1, CourseId = course.Id, SortOrder = 1 };
            DbContext.CourseSections.Add(section1);
            await DbContext.SaveChangesAsync();

            var request = new CreateSectionRequest { Title = "S2", CourseId = course.Id,Description="Desc" };
            var result = await _sectionService.CreateSectionAsync(request);

            Assert.That(result.SortOrder, Is.EqualTo(2));
        }

        // ─── UpdateSectionAsync ────────────────────────────────────────────────

        [Test]
        public async Task UpdateSectionAsync_ValidUpdate_UpdatesSection()
        {
            var course = await CreateTestCourse();
            var section = new CourseSection { Title = "Old Title", Description = "Desc", SectionId = 1, CourseId = course.Id, SortOrder = 1 };
            DbContext.CourseSections.Add(section);
            await DbContext.SaveChangesAsync();

            var request = new UpdateSectionRequest { Title = "New Title", SortOrder = 2 };

            var result = await _sectionService.UpdateSectionAsync(section.Id, request);

            Assert.That(result.Title, Is.EqualTo("New Title"));
            Assert.That(result.SortOrder, Is.EqualTo(2));
        }

        [Test]
        public async Task UpdateSectionAsync_ManualStatusChangeSelfPaced_ThrowsInvalidOperationException()
        {
            var course = await CreateTestCourse(CourseAccessType.SelfPaced);
            var section = new CourseSection { Title = "Sec", Description = "Desc", SectionId = 1, CourseId = course.Id };
            DbContext.CourseSections.Add(section);
            await DbContext.SaveChangesAsync();

            // Trying to manually set status on a SelfPaced course should throw
            var request = new UpdateSectionRequest { Status = PublishStatus.Published };
            Assert.ThrowsAsync<InvalidOperationException>(() => _sectionService.UpdateSectionAsync(section.Id, request));
        }

        // ─── DeleteSectionAsync ────────────────────────────────────────────────

        [Test]
        public async Task DeleteSectionAsync_DeletesSection()
        {
            var course = await CreateTestCourse();
            var section = new CourseSection { Title = "To Delete", Description = "Desc", SectionId = 1, CourseId = course.Id };
            DbContext.CourseSections.Add(section);
            await DbContext.SaveChangesAsync();

            await _sectionService.DeleteSectionAsync(section.Id);

            var dbSection = await DbContext.CourseSections.FindAsync(section.Id);
            Assert.That(dbSection, Is.Null);
        }

        [Test]
        public void DeleteSectionAsync_NotFound_ThrowsKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _sectionService.DeleteSectionAsync(99999));
        }

        // ─── ReorderSectionsAsync ──────────────────────────────────────────────

        [Test]
        public async Task ReorderSectionsAsync_ReordersSections()
        {
            var course = await CreateTestCourse();
            var section1 = new CourseSection { Title = "Section 1", Description = "Desc", SectionId = 1, CourseId = course.Id, SortOrder = 1 };
            var section2 = new CourseSection { Title = "Section 2", Description = "Desc", SectionId = 2, CourseId = course.Id, SortOrder = 2 };
            DbContext.CourseSections.Add(section1);
            DbContext.CourseSections.Add(section2);
            await DbContext.SaveChangesAsync();

            var request = new ReorderSectionsRequest 
            {
                SectionOrders = new System.Collections.Generic.List<SectionOrderItem>
                {
                    new SectionOrderItem { SectionId = section1.Id, SortOrder = 2 },
                    new SectionOrderItem { SectionId = section2.Id, SortOrder = 1 }
                }
            };

            await _sectionService.ReorderSectionsAsync(request);

            var s1 = await DbContext.CourseSections.FindAsync(section1.Id);
            var s2 = await DbContext.CourseSections.FindAsync(section2.Id);

            Assert.That(s1!.SortOrder, Is.EqualTo(2));
            Assert.That(s2!.SortOrder, Is.EqualTo(1));
        }

        [Test]
        public void ReorderSectionsAsync_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _sectionService.ReorderSectionsAsync(null!));
        }

        // ─── PublishSectionAsync ───────────────────────────────────────────────

        [Test]
        public async Task PublishSectionAsync_SelfPacedCourse_ThrowsInvalidOperationException()
        {
            var course = await CreateTestCourse(CourseAccessType.SelfPaced);
            var section = new CourseSection { Title = "Sec", Description = "Desc", SectionId = 1, CourseId = course.Id };
            DbContext.CourseSections.Add(section);
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sectionService.PublishSectionAsync(section.Id, new PublishSectionRequest { Publish = true }));
        }

        [Test]
        public async Task PublishSectionAsync_CohortBased_PublishesSection()
        {
            var course = await CreateTestCourse(CourseAccessType.CohortBased);
            var section = new CourseSection { Title = "Sec", Description = "Desc", SectionId = 1, CourseId = course.Id, Status = PublishStatus.Draft };
            DbContext.CourseSections.Add(section);
            await DbContext.SaveChangesAsync();

            var result = await _sectionService.PublishSectionAsync(section.Id, new PublishSectionRequest { Publish = true });

            Assert.That(result.Status, Is.EqualTo(PublishStatus.Published));
        }

        // ─── GetSectionsByCourseAsync ──────────────────────────────────────────

        [Test]
        public async Task GetSectionsByCourseAsync_NonInstructorSeesOnlyPublished()
        {
            var course = await CreateTestCourse(CourseAccessType.CohortBased);
            var pubSection = new CourseSection { Title = "Published", Description = "D", SectionId = 1, CourseId = course.Id, Status = PublishStatus.Published };
            var draftSection = new CourseSection { Title = "Draft", Description = "D", SectionId = 2, CourseId = course.Id, Status = PublishStatus.Draft };
            DbContext.CourseSections.AddRange(pubSection, draftSection);
            await DbContext.SaveChangesAsync();

            // currentUserId = null → non-instructor view
            var result = (await _sectionService.GetSectionsByCourseAsync(course.Id, currentUserId: null)).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("Published"));
        }
    }
}
