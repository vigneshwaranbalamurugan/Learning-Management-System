using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Services;
using LMSApi.DALLibrary.Repositories;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.Enums;
using NUnit.Framework;

namespace LMSApi.Tests.Services
{
    [TestFixture]
    public class WishListServiceTests : BaseServiceTest
    {
        private IWishListService _wishListService = null!;
        private Users _user = null!;
        private Users _instructor = null!;
        private Courses _course = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            var wishListRepository = new WishListRepository(DbContext);
            var enrollmentRepository = new EnrollmentRepository(DbContext);
            var courseRepository = new CourseRepository(DbContext);

            _wishListService = new WishListService(
                wishListRepository,
                enrollmentRepository,
                courseRepository
            );

            // Seed common entities safely
            var instructorRole = DbContext.UserRoles.FirstOrDefault(r => r.RoleName == "Instructor") 
                ?? new UserRoles { RoleName = "Instructor", Description = "Instructor role" };
            var learnerRole = DbContext.UserRoles.FirstOrDefault(r => r.RoleName == "Learner") 
                ?? new UserRoles { RoleName = "Learner", Description = "Learner role" };
            
            if (instructorRole.Id == 0) DbContext.UserRoles.Add(instructorRole);
            if (learnerRole.Id == 0) DbContext.UserRoles.Add(learnerRole);
            DbContext.SaveChanges();

            _instructor = new Users
            {
                Email = "instructor_" + Guid.NewGuid().ToString("N") + "@test.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                RoleId = instructorRole.Id
            };

            _user = new Users
            {
                Email = "learner_" + Guid.NewGuid().ToString("N") + "@test.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                RoleId = learnerRole.Id
            };

            DbContext.Users.AddRange(_instructor, _user);
            DbContext.SaveChanges();

            var cat = DbContext.CourseCategories.FirstOrDefault(c => c.Name == "Art") 
                ?? new CourseCategories { Name = "Art", Description = "Art courses" };
            if (cat.Id == 0) DbContext.CourseCategories.Add(cat);

            var lang = DbContext.CourseLanguages.FirstOrDefault(l => l.Name == "English") 
                ?? new CourseLanguages { Name = "English" };
            if (lang.Id == 0) DbContext.CourseLanguages.Add(lang);
            DbContext.SaveChanges();

            _course = new Courses
            {
                Title = "Paint Course",
                Description = "A paint course description",
                Price = 15.0m,
                IsPremium = true,
                ThumbnailUrl = "thumbnail.png",
                IntroVideoUrl = "video.mp4",
                Requirements = "None",
                LearningOutcomes = "Knowledge",
                EstimatedDuration = TimeSpan.FromHours(5),
                Level = CourseLevel.Beginner,
                LanguageId = lang.Id,
                CategoryId = cat.Id,
                InstructorId = _instructor.Id,
                slug = "paint-course-slug"
            };

            DbContext.Courses.Add(_course);
            DbContext.SaveChanges();
        }

        [Test]
        public async Task AddToWishListAsync_ValidRequest_AddsCourse()
        {
            var request = new AddWishListRequest
            {
                CourseId = _course.Id
            };

            var response = await _wishListService.AddToWishListAsync(_user.Id, request);

            Assert.That(response, Is.Not.Null);
            Assert.That(response.CourseId, Is.EqualTo(_course.Id));
            Assert.That(response.CourseTitle, Is.EqualTo(_course.Title));

            // Verify in DB
            var exists = DbContext.WishLists.Any(w => w.UserId == _user.Id && w.CourseId == _course.Id);
            Assert.That(exists, Is.True);
        }

        [Test]
        public async Task AddToWishListAsync_AlreadyEnrolled_ThrowsInvalidOperationException()
        {
            // Set up enrollment
            DbContext.Enrollments.Add(new Enrollments
            {
                UserId = _user.Id,
                CourseId = _course.Id,
                EnrolledAt = DateTime.UtcNow,
                EnrollmentStatus = LMSApi.ModelLibrary.Enums.EnrollmentStatus.Active
            });
            await DbContext.SaveChangesAsync();

            var request = new AddWishListRequest { CourseId = _course.Id };

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _wishListService.AddToWishListAsync(_user.Id, request));
        }

        [Test]
        public async Task AddToWishListAsync_AlreadyWishlisted_ThrowsInvalidOperationException()
        {
            DbContext.WishLists.Add(new WishList
            {
                UserId = _user.Id,
                CourseId = _course.Id
            });
            await DbContext.SaveChangesAsync();

            var request = new AddWishListRequest { CourseId = _course.Id };

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _wishListService.AddToWishListAsync(_user.Id, request));
        }

        [Test]
        public async Task RemoveFromWishListAsync_ExistingItem_RemovesCourse()
        {
            var wishlist = new WishList
            {
                UserId = _user.Id,
                CourseId = _course.Id
            };
            DbContext.WishLists.Add(wishlist);
            await DbContext.SaveChangesAsync();

            await _wishListService.RemoveFromWishListAsync(_user.Id, _course.Id);

            var exists = DbContext.WishLists.Any(w => w.Id == wishlist.Id);
            Assert.That(exists, Is.False);
        }

        [Test]
        public async Task GetUserWishListAsync_ReturnsAllUserItems()
        {
            DbContext.WishLists.Add(new WishList
            {
                UserId = _user.Id,
                CourseId = _course.Id
            });
            await DbContext.SaveChangesAsync();

            var items = await _wishListService.GetUserWishListAsync(_user.Id);

            Assert.That(items.Count(), Is.EqualTo(1));
            Assert.That(items.First().CourseTitle, Is.EqualTo(_course.Title));
        }
    }
}
