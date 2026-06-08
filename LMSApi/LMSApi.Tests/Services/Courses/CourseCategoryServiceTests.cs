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
    public class CourseCategoryServiceTests : BaseServiceTest
    {
        private Mock<ILogger<CourseCategoryService>> _mockLogger = null!;
        private ICourseCategoryService _courseCategoryService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mockLogger = new Mock<ILogger<CourseCategoryService>>();
            
            var categoryRepository = new CourseCategoryRepository(DbContext);
            var courseRepository = new CourseRepository(DbContext);

            _courseCategoryService = new CourseCategoryService(
                categoryRepository,
                courseRepository,
                Mapper,
                _mockLogger.Object
            );
        }

        [Test]
        public async Task GetAllCategoriesAsync_ReturnsAllCategories()
        {
            DbContext.CourseCategories.Add(new CourseCategories { Name = "Cat1", Description = "Desc1" });
            DbContext.CourseCategories.Add(new CourseCategories { Name = "Cat2", Description = "Desc2" });
            await DbContext.SaveChangesAsync();

            var result = await _courseCategoryService.GetAllCategoriesAsync();

            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task CreateCategoryAsync_ValidRequest_CreatesAndReturnsCategory()
        {
            var request = new CreateCategoryRequest { Name = "NewCat", Description = "NewDesc" };

            var result = await _courseCategoryService.CreateCategoryAsync(request);

            Assert.That(result.Name, Is.EqualTo("NewCat"));
            var dbCat = await DbContext.CourseCategories.FindAsync(result.Id);
            Assert.That(dbCat, Is.Not.Null);
        }

        [Test]
        public async Task CreateCategoryAsync_DuplicateName_ThrowsInvalidOperationException()
        {
            DbContext.CourseCategories.Add(new CourseCategories { Name = "DuplicateCat", Description = "Desc" });
            await DbContext.SaveChangesAsync();

            var request = new CreateCategoryRequest { Name = "DuplicateCat" };

            Assert.ThrowsAsync<InvalidOperationException>(() => _courseCategoryService.CreateCategoryAsync(request));
        }

        [Test]
        public async Task UpdateCategoryAsync_ValidRequest_UpdatesAndReturnsCategory()
        {
            var cat = new CourseCategories { Name = "OldName", Description = "OldDesc" };
            DbContext.CourseCategories.Add(cat);
            await DbContext.SaveChangesAsync();

            var request = new UpdateCategoryRequest { Name = "UpdatedName" };

            var result = await _courseCategoryService.UpdateCategoryAsync(cat.Id, request);

            Assert.That(result.Name, Is.EqualTo("UpdatedName"));
        }

        [Test]
        public async Task DeleteCategoryAsync_NoLinkedCourses_DeletesCategory()
        {
            var cat = new CourseCategories { Name = "ToDelete", Description = "Desc" };
            DbContext.CourseCategories.Add(cat);
            await DbContext.SaveChangesAsync();

            await _courseCategoryService.DeleteCategoryAsync(cat.Id);

            var dbCat = await DbContext.CourseCategories.FindAsync(cat.Id);
            Assert.That(dbCat, Is.Null);
        }

        [Test]
        public async Task DeleteCategoryAsync_WithLinkedCourses_ThrowsInvalidOperationException()
        {
            var cat = new CourseCategories { Name = "ToDeleteWithCourses", Description = "Desc" };
            DbContext.CourseCategories.Add(cat);
            await DbContext.SaveChangesAsync();

            var user = new Users { Email = "test@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Instructor", Description = "Desc" } };
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

            DbContext.Courses.Add(new LMSApi.ModelLibrary.Models.Courses 
            { 
                Title = "Course1", 
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
                slug = "course-1" 
            });
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() => _courseCategoryService.DeleteCategoryAsync(cat.Id));
        }
    }
}
