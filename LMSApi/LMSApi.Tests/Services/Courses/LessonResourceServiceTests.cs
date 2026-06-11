using System;
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
    public class LessonResourceServiceTests : BaseServiceTest
    {
        private Mock<ILogger<LessonResourceService>> _mockLogger = null!;
        private Mock<IUploadService> _mockUploadService = null!;
        private ILessonResourceService _resourceService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mockLogger = new Mock<ILogger<LessonResourceService>>();
            _mockUploadService = new Mock<IUploadService>();
            
            var resourceRepository = new LessonResourceRepository(DbContext);
            var lessonRepository = new LessonRepository(DbContext);
            var sectionRepository = new CourseSectionRepository(DbContext);
            var courseRepository = new CourseRepository(DbContext);

            _resourceService = new LessonResourceService(
                resourceRepository,
                lessonRepository,
                sectionRepository,
                courseRepository,
                _mockUploadService.Object,
                Mapper,
                _mockLogger.Object
            );
        }

        [Test]
        public async Task DeleteResourceAsync_DeletesResource()
        {
            var inst = new Users { Email = "inst@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Instructor", Description = "Desc" } };
            var cat = new CourseCategories { Name = "Cat", Description = "Desc" };
            DbContext.Users.Add(inst);
            DbContext.CourseCategories.Add(cat);
            await DbContext.SaveChangesAsync();

            var course = new LMSApi.ModelLibrary.Models.Courses { Title = "Course", Description = "Desc", Price = 0m, ThumbnailUrl = "url", IntroVideoUrl = "url", IsPremium = false, Requirements = "Reqs", LearningOutcomes = "Outcomes", EstimatedDuration = TimeSpan.Zero, Level = LMSApi.ModelLibrary.Enums.CourseLevel.Beginner, Language = LMSApi.ModelLibrary.Enums.CourseLanguage.English, PublishedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, DefaultDeadlineDays = 7, CategoryId = cat.Id, InstructorId = inst.Id, slug = Guid.NewGuid().ToString() };
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            var section = new CourseSection { Title = "Sec", Description = "Desc", SectionId = 1, CourseId = course.Id };
            DbContext.CourseSections.Add(section);
            await DbContext.SaveChangesAsync();

            var lesson = new Lessons { Title = "Lesson", Description = "Desc", Content = "Content", ContentUrl = "url", CourseSectionId = section.Id };
            DbContext.Lessons.Add(lesson);
            await DbContext.SaveChangesAsync();

            var resource = new LessonResources { LessonId = lesson.Id, ResourceUrl = "url", ResourceTitle = "Title", Description = "Desc", ResourceType = LMSApi.ModelLibrary.Enums.ResourceType.Pdf };
            DbContext.LessonResources.Add(resource);
            await DbContext.SaveChangesAsync();

            await _resourceService.DeleteResourceAsync(resource.Id);

            var dbRes = await DbContext.LessonResources.FindAsync(resource.Id);
            Assert.That(dbRes, Is.Null);
        }
    }
}
