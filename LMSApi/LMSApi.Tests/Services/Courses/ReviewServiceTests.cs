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
    public class ReviewServiceTests : BaseServiceTest
    {
        private IReviewService _reviewService = null!;
        private Users _student = null!;
        private Users _instructor = null!;
        private Courses _course = null!;
        private int _lessonId;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            var reviewRepository = new ReviewRepository(DbContext);
            var progressRepository = new StudentProgressRepository(DbContext);
            var courseRepository = new CourseRepository(DbContext);
            var userRepository = new UserRepository(DbContext);

            _reviewService = new ReviewService(
                reviewRepository,
                progressRepository,
                courseRepository,
                userRepository
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

            _student = new Users
            {
                Email = "student_" + Guid.NewGuid().ToString("N") + "@test.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                RoleId = learnerRole.Id
            };

            DbContext.Users.AddRange(_instructor, _student);
            DbContext.SaveChanges();

            var cat = DbContext.CourseCategories.FirstOrDefault(c => c.Name == "Tech") 
                ?? new CourseCategories { Name = "Tech", Description = "Tech courses" };
            if (cat.Id == 0) DbContext.CourseCategories.Add(cat);

            var lang = DbContext.CourseLanguages.FirstOrDefault(l => l.Name == "English") 
                ?? new CourseLanguages { Name = "English" };
            if (lang.Id == 0) DbContext.CourseLanguages.Add(lang);
            DbContext.SaveChanges();

            _course = new Courses
            {
                Title = "Test Course",
                Description = "A test course description",
                Price = 10.0m,
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
                slug = "test-course-slug"
            };

            DbContext.Courses.Add(_course);
            DbContext.SaveChanges();

            var section = new CourseSection { Title = "Intro", Description = "Intro section", CourseId = _course.Id };
            DbContext.CourseSections.Add(section);
            DbContext.SaveChanges();

            var lesson = new Lessons { Title = "Lesson 1", Description = "Description", CourseSectionId = section.Id };
            DbContext.Lessons.Add(lesson);
            DbContext.SaveChanges();

            _lessonId = lesson.Id;
        }

        [Test]
        public async Task AddReviewAsync_ValidRequest_CreatesReview()
        {
            // Set student progress
            DbContext.StudentProgresses.Add(new StudentProgress
            {
                StudentId = _student.Id,
                CourseId = _course.Id,
                LessonId = _lessonId, // Real Lesson ID
                VideoWatchedPercentage = 50,
                IsCompleted = false,
                LastAccessed = DateTime.UtcNow
            });
            await DbContext.SaveChangesAsync();

            var request = new CreateReviewRequest
            {
                CourseId = _course.Id,
                Rating = 5,
                ReviewText = "Excellent course!"
            };

            var response = await _reviewService.AddReviewAsync(_student.Id, request);

            Assert.That(response, Is.Not.Null);
            Assert.That(response.Rating, Is.EqualTo(5));
            Assert.That(response.ReviewText, Is.EqualTo("Excellent course!"));
            Assert.That(response.UserId, Is.EqualTo(_student.Id));

            // Verify in DB
            var dbReview = DbContext.Reviews.FirstOrDefault(r => r.Id == response.Id);
            Assert.That(dbReview, Is.Not.Null);
            Assert.That(dbReview.Rating, Is.EqualTo(5));
        }

        [Test]
        public void AddReviewAsync_NoProgress_ThrowsInvalidOperationException()
        {
            // Student has no progress records in the course
            var request = new CreateReviewRequest
            {
                CourseId = _course.Id,
                Rating = 4,
                ReviewText = "No progress review"
            };

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reviewService.AddReviewAsync(_student.Id, request));
        }

        [Test]
        public async Task AddReviewAsync_AlreadyReviewed_ThrowsInvalidOperationException()
        {
            // Set student progress
            DbContext.StudentProgresses.Add(new StudentProgress
            {
                StudentId = _student.Id,
                CourseId = _course.Id,
                LessonId = _lessonId,
                LastAccessed = DateTime.UtcNow
            });
            // Add existing review
            DbContext.Reviews.Add(new Reviews
            {
                UserId = _student.Id,
                CourseId = _course.Id,
                Rating = 4,
                Review = "First review"
            });
            await DbContext.SaveChangesAsync();

            var request = new CreateReviewRequest
            {
                CourseId = _course.Id,
                Rating = 5,
                ReviewText = "Duplicate review text"
            };

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _reviewService.AddReviewAsync(_student.Id, request));
        }

        [Test]
        public async Task UpdateReviewAsync_ValidRequest_UpdatesReview()
        {
            var review = new Reviews
            {
                UserId = _student.Id,
                CourseId = _course.Id,
                Rating = 3,
                Review = "Initial feedback"
            };
            DbContext.Reviews.Add(review);
            await DbContext.SaveChangesAsync();

            var request = new UpdateReviewRequest
            {
                Rating = 4,
                ReviewText = "Updated feedback!"
            };

            var response = await _reviewService.UpdateReviewAsync(_student.Id, review.Id, request);

            Assert.That(response.Rating, Is.EqualTo(4));
            Assert.That(response.ReviewText, Is.EqualTo("Updated feedback!"));

            // Verify in DB
            var dbReview = DbContext.Reviews.Find(review.Id);
            Assert.That(dbReview!.Rating, Is.EqualTo(4));
        }

        [Test]
        public void UpdateReviewAsync_NonExistentReview_ThrowsKeyNotFoundException()
        {
            var request = new UpdateReviewRequest { Rating = 5 };

            Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _reviewService.UpdateReviewAsync(_student.Id, 99999, request));
        }

        [Test]
        public async Task DeleteReviewAsync_ValidRequest_DeletesReview()
        {
            var review = new Reviews
            {
                UserId = _student.Id,
                CourseId = _course.Id,
                Rating = 4,
                Review = "To delete"
            };
            DbContext.Reviews.Add(review);
            await DbContext.SaveChangesAsync();

            await _reviewService.DeleteReviewAsync(_student.Id, review.Id);

            var dbReview = DbContext.Reviews.Find(review.Id);
            Assert.That(dbReview, Is.Null);
        }

        [Test]
        public async Task GetCourseReviewsAsync_ReturnsAllCourseReviews()
        {
            DbContext.Reviews.Add(new Reviews { UserId = _student.Id, CourseId = _course.Id, Rating = 5, Review = "Great" });
            await DbContext.SaveChangesAsync();

            var reviews = await _reviewService.GetCourseReviewsAsync(_course.Id);

            Assert.That(reviews.Count(), Is.EqualTo(1));
            Assert.That(reviews.First().ReviewText, Is.EqualTo("Great"));
        }
    }
}
