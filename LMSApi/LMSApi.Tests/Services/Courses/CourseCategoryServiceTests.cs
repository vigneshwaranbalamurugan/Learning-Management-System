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

namespace LMSApi.Tests.Services
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

        // ─── GetAllCategoriesAsync ─────────────────────────────────────────────

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
        public async Task GetAllCategoriesAsync_EmptyDatabase_ReturnsEmptyList()
        {
            var result = await _courseCategoryService.GetAllCategoriesAsync();
            Assert.That(result, Is.Empty);
        }

        // ─── GetCategoryByIdAsync ──────────────────────────────────────────────

        [Test]
        public void GetCategoryByIdAsync_NotFound_ThrowsKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _courseCategoryService.GetCategoryByIdAsync(99999));
        }

        [Test]
        public async Task GetCategoryByIdAsync_ExistingId_ReturnsCategory()
        {
            var cat = new CourseCategories { Name = "FindMe", Description = "Desc" };
            DbContext.CourseCategories.Add(cat);
            await DbContext.SaveChangesAsync();

            var result = await _courseCategoryService.GetCategoryByIdAsync(cat.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("FindMe"));
        }

        // ─── CreateCategoryAsync ───────────────────────────────────────────────

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

        // ─── UpdateCategoryAsync ───────────────────────────────────────────────

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
        public async Task UpdateCategoryAsync_DuplicateName_ThrowsInvalidOperationException()
        {
            var cat1 = new CourseCategories { Name = "ExistingName", Description = "Desc" };
            var cat2 = new CourseCategories { Name = "OtherName", Description = "Desc" };
            DbContext.CourseCategories.AddRange(cat1, cat2);
            await DbContext.SaveChangesAsync();

            // Try to rename cat2 to the name of cat1
            var request = new UpdateCategoryRequest { Name = "ExistingName" };
            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _courseCategoryService.UpdateCategoryAsync(cat2.Id, request));
        }

        [Test]
        public void UpdateCategoryAsync_NotFound_ThrowsKeyNotFoundException()
        {
            var request = new UpdateCategoryRequest { Name = "Ghost" };
            Assert.ThrowsAsync<KeyNotFoundException>(() => _courseCategoryService.UpdateCategoryAsync(99999, request));
        }

        [Test]
        public async Task UpdateCategoryAsync_SameName_DoesNotThrow()
        {
            var cat = new CourseCategories { Name = "SameName", Description = "Desc" };
            DbContext.CourseCategories.Add(cat);
            await DbContext.SaveChangesAsync();

            // Updating with same name should not throw (the service excludes self from uniqueness check)
            var request = new UpdateCategoryRequest { Name = "SameName", Description = "Updated Desc" };
            var result = await _courseCategoryService.UpdateCategoryAsync(cat.Id, request);
            Assert.That(result.Description, Is.EqualTo("Updated Desc"));
        }

        // ─── DeleteCategoryAsync ───────────────────────────────────────────────

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

            DbContext.Courses.Add(new Courses 
            { 
                Title = "Course1", 
                Description = "Desc", 
                Price = 0m,
                ThumbnailUrl = "url",
                IntroVideoUrl = "url", 
                IsPremium = false,
                Requirements = "Reqs",
                LearningOutcomes = "Outcomes",
                EstimatedDuration = TimeSpan.Zero,
                Level = LMSApi.ModelLibrary.Enums.CourseLevel.Beginner,
                LanguageId = 1,
                PublishedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                DefaultDeadlineDays = 7,
                CategoryId = cat.Id, 
                InstructorId = user.Id, 
                slug = "course-1" 
            });
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() => _courseCategoryService.DeleteCategoryAsync(cat.Id));
        }

        [Test]
        public void DeleteCategoryAsync_NotFound_ThrowsKeyNotFoundException()
        {
            Assert.ThrowsAsync<KeyNotFoundException>(() => _courseCategoryService.DeleteCategoryAsync(99999));
        }
    }
}
