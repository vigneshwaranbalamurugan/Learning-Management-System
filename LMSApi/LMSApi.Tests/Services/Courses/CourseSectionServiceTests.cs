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

            _sectionService = new CourseSectionService(
                sectionRepository,
                courseRepository,
                Mapper,
                _mockLogger.Object
            );
        }

        private async Task<LMSApi.ModelLibrary.Models.Courses> CreateTestCourse(CourseAccessType accessType = CourseAccessType.SelfPaced)
        {
            var cat = new CourseCategories { Name = "Cat", Description = "Desc" };
            var user = new Users { Email = "test@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Instructor", Description = "Desc" } };
            DbContext.CourseCategories.Add(cat);
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

            var course = new LMSApi.ModelLibrary.Models.Courses 
            { 
                Title = "Test Course", 
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
                InstructorId = user.Id, 
                slug = Guid.NewGuid().ToString(),
                CourseAccessType = accessType,
                Status = CourseStatus.Draft
            };
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();
            return course;
        }

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
    }
}
