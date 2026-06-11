using System;
using System.Linq;
using System.Threading.Tasks;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Services;
using LMSApi.DALLibrary.Repositories;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LMSApi.Tests.Services.Courses
{
    [TestFixture]
    public class LessonServiceTests : BaseServiceTest
    {
        private Mock<ILogger<LessonService>> _mockLogger = null!;
        private Mock<IUploadService> _mockUploadService = null!;
        private ILessonService _lessonService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mockLogger = new Mock<ILogger<LessonService>>();
            _mockUploadService = new Mock<IUploadService>();
            
            var lessonRepository = new LessonRepository(DbContext);
            var sectionRepository = new CourseSectionRepository(DbContext);
            var courseRepository = new CourseRepository(DbContext);

            _lessonService = new LessonService(
                lessonRepository,
                sectionRepository,
                courseRepository,
                _mockUploadService.Object,
                Mapper,
                _mockLogger.Object
            );
        }

        private async Task<CourseSection> SetupSection()
        {
            var cat = new CourseCategories { Name = "Cat", Description = "Desc" };
            var inst = new Users { Email = "inst@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Instructor", Description = "Desc" } };
            DbContext.Users.Add(inst);
            DbContext.CourseCategories.Add(cat);
            await DbContext.SaveChangesAsync();

            var course = new LMSApi.ModelLibrary.Models.Courses { Title = "Course", Description = "Desc", Price = 0m, ThumbnailUrl = "url", IntroVideoUrl = "url", IsPremium = false, Requirements = "Reqs", LearningOutcomes = "Outcomes", EstimatedDuration = TimeSpan.Zero, Level = LMSApi.ModelLibrary.Enums.CourseLevel.Beginner, Language = LMSApi.ModelLibrary.Enums.CourseLanguage.English, PublishedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, DefaultDeadlineDays = 7, CategoryId = cat.Id, InstructorId = inst.Id, slug = Guid.NewGuid().ToString() };
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            var section = new CourseSection { Title = "Section 1", Description = "Desc", SectionId = 1, CourseId = course.Id };
            DbContext.CourseSections.Add(section);
            await DbContext.SaveChangesAsync();

            return section;
        }

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
    }
}
