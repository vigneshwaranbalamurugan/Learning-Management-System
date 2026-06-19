using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Services;
using LMSApi.DALLibrary.Repositories;
using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;

using NUnit.Framework;

namespace LMSApi.Tests.Services
{
    [TestFixture]
    public class AnalyticsServiceTests : BaseServiceTest
    {
        private IAnalyticsService _analyticsService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            var analyticsRepository = new AnalyticsRepository(DbContext);
            _analyticsService = new AnalyticsService(analyticsRepository);
        }

        [Test]
        public async Task GetAdminAnalyticsAsync_ReturnsCorrectMetrics()
        {
            // Arrange
            var learnerRole = new UserRoles { RoleName = "Learner", Description = "Learner Role" };
            var instructorRole = new UserRoles { RoleName = "Instructor", Description = "Instructor Role" };
            DbContext.UserRoles.AddRange(learnerRole, instructorRole);

            var learner = new Users { Email = "learner@test.com", PasswordHash = "h", PasswordSalt = "s", Role = learnerRole };
            var instructor = new Users { Email = "instructor@test.com", PasswordHash = "h", PasswordSalt = "s", Role = instructorRole };
            DbContext.Users.AddRange(learner, instructor);
            await DbContext.SaveChangesAsync();

            var category = new CourseCategories { Name = "Tech", Description = "Tech courses" };
            DbContext.CourseCategories.Add(category);
            await DbContext.SaveChangesAsync();

            var course = new Courses
            {
                Title = "C# Course", Description = "Learn C#", Price = 99.99m, IsPremium = true, ThumbnailUrl = "url",
                Requirements = "None", LearningOutcomes = "C#", EstimatedDuration = TimeSpan.FromHours(5),
                Level = CourseLevel.Beginner, LanguageId = 1, Status = CourseStatus.Published,
                CategoryId = category.Id, InstructorId = instructor.Id, slug = "csharp-course"
            };
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            var enrollment = new Enrollments
            {
                UserId = learner.Id, CourseId = course.Id, EnrolledAt = DateTime.UtcNow,
                ProgressPercentage = 50, IsCompleted = false, EnrollmentStatus = EnrollmentStatus.Active
            };
            DbContext.Enrollments.Add(enrollment);
            await DbContext.SaveChangesAsync();

            var payment = new Payments
            {
                UserId = learner.Id, CourseId = course.Id, EnrollmentId = enrollment.Id,
                ProviderOrderId = "order_123", Amount = 99.99m, Status = PaymentStatus.Completed, PaidAt = DateTime.UtcNow
            };
            DbContext.Payments.Add(payment);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _analyticsService.GetAdminAnalyticsAsync();

            // Assert
            Assert.That(result.TotalUsers, Is.EqualTo(3)); // 2 created + 1 seeded admin
            Assert.That(result.TotalLearners, Is.EqualTo(1));
            Assert.That(result.TotalInstructors, Is.EqualTo(1));
            Assert.That(result.TotalCourses, Is.EqualTo(1));
            Assert.That(result.ActiveCourses, Is.EqualTo(1));
            Assert.That(result.TotalEnrollments, Is.EqualTo(1));
            Assert.That(result.TotalRevenue, Is.EqualTo(99.99m));
        }

        [Test]
        public async Task GetInstructorAnalyticsAsync_ReturnsCorrectMetrics()
        {
            // Arrange
            var learnerRole = new UserRoles { RoleName = "Learner", Description = "Learner Role" };
            var instructorRole = new UserRoles { RoleName = "Instructor", Description = "Instructor Role" };
            DbContext.UserRoles.AddRange(learnerRole, instructorRole);

            var learner = new Users { Email = "learner@test.com", PasswordHash = "h", PasswordSalt = "s", Role = learnerRole };
            var instructor = new Users { Email = "instructor@test.com", PasswordHash = "h", PasswordSalt = "s", Role = instructorRole };
            DbContext.Users.AddRange(learner, instructor);
            await DbContext.SaveChangesAsync();

            var category = new CourseCategories { Name = "Tech", Description = "Tech courses" };
            DbContext.CourseCategories.Add(category);
            await DbContext.SaveChangesAsync();

            var course = new Courses
            {
                Title = "C# Course", Description = "Learn C#", Price = 99.99m, IsPremium = true, ThumbnailUrl = "url",
                Requirements = "None", LearningOutcomes = "C#", EstimatedDuration = TimeSpan.FromHours(5),
                Level = CourseLevel.Beginner, LanguageId = 1, Status = CourseStatus.Published,
                CategoryId = category.Id, InstructorId = instructor.Id, slug = "csharp-course"
            };
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            var enrollment = new Enrollments
            {
                UserId = learner.Id, CourseId = course.Id, EnrolledAt = DateTime.UtcNow,
                ProgressPercentage = 100, IsCompleted = true, EnrollmentStatus = EnrollmentStatus.Completed
            };
            DbContext.Enrollments.Add(enrollment);
            await DbContext.SaveChangesAsync();

            var payment = new Payments
            {
                UserId = learner.Id, CourseId = course.Id, EnrollmentId = enrollment.Id,
                ProviderOrderId = "order_123", Amount = 99.99m, Status = PaymentStatus.Completed, PaidAt = DateTime.UtcNow
            };
            DbContext.Payments.Add(payment);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _analyticsService.GetInstructorAnalyticsAsync(instructor.Id);

            // Assert
            Assert.That(result.TotalCoursesCreated, Is.EqualTo(1));
            Assert.That(result.TotalStudentsEnrolled, Is.EqualTo(1));
            Assert.That(result.TotalRevenueGenerated, Is.EqualTo(99.99m));
            Assert.That(result.AverageQuizScore, Is.Null);
            Assert.That(result.AverageAssignmentScore, Is.Null);
        }

        [Test]
        public async Task GetLearnerAnalyticsAsync_ReturnsCorrectMetrics()
        {
            // Arrange
            var learnerRole = new UserRoles { RoleName = "Learner", Description = "Learner Role" };
            DbContext.UserRoles.Add(learnerRole);

            var learner = new Users { Email = "learner@test.com", PasswordHash = "h", PasswordSalt = "s", Role = learnerRole };
            DbContext.Users.Add(learner);
            await DbContext.SaveChangesAsync();

            var category = new CourseCategories { Name = "Tech", Description = "Tech courses" };
            DbContext.CourseCategories.Add(category);
            await DbContext.SaveChangesAsync();

            var course = new Courses
            {
                Title = "C# Course", Description = "Learn C#", Price = 99.99m, IsPremium = true, ThumbnailUrl = "url",
                Requirements = "None", LearningOutcomes = "C#", EstimatedDuration = TimeSpan.FromHours(5),
                Level = CourseLevel.Beginner, LanguageId = 1, Status = CourseStatus.Published,
                CategoryId = category.Id, InstructorId = 1, slug = "csharp-course"
            };
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            var enrollment = new Enrollments
            {
                UserId = learner.Id, CourseId = course.Id, EnrolledAt = DateTime.UtcNow,
                ProgressPercentage = 80, IsCompleted = false, EnrollmentStatus = EnrollmentStatus.Active
            };
            DbContext.Enrollments.Add(enrollment);
            await DbContext.SaveChangesAsync();

            // Act
            var result = await _analyticsService.GetLearnerAnalyticsAsync(learner.Id);

            // Assert
            Assert.That(result.TotalEnrolledCourses, Is.EqualTo(1));
            Assert.That(result.CompletedCourses, Is.EqualTo(0));
            Assert.That(result.InProgressCourses, Is.EqualTo(1));
            Assert.That(result.AverageProgressPercentage, Is.EqualTo(80m));
            Assert.That(result.AverageQuizScore, Is.Null);
            Assert.That(result.AverageAssignmentScore, Is.Null);
        }
    }
}
