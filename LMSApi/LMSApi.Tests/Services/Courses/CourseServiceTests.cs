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
    public class CourseServiceTests : BaseServiceTest
    {
        private Mock<ILogger<CourseService>> _mockLogger = null!;
        private Mock<IUploadService> _mockUploadService = null!;
        private ICourseService _courseService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mockLogger = new Mock<ILogger<CourseService>>();
            _mockUploadService = new Mock<IUploadService>();
            
            var courseRepository = new CourseRepository(DbContext);
            var categoryRepository = new CourseCategoryRepository(DbContext);
            var userRepository = new UserRepository(DbContext);
            var enrollmentRepository = new EnrollmentRepository(DbContext);

            _courseService = new CourseService(
                courseRepository,
                categoryRepository,
                userRepository,
                enrollmentRepository,
                _mockUploadService.Object,
                Mapper,
                _mockLogger.Object
            );
        }

        private async Task<(Users user, CourseCategories category)> CreateUserAndCategory()
        {
            var user = new Users { Email = "instr@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Instructor", Description = "Desc" } };
            var cat = new CourseCategories { Name = "Tech", Description = "Desc" };
            DbContext.Users.Add(user);
            DbContext.CourseCategories.Add(cat);
            await DbContext.SaveChangesAsync();
            return (user, cat);
        }

        [Test]
        public async Task CreateCourseAsync_ValidRequest_CreatesCourse()
        {
            var (user, cat) = await CreateUserAndCategory();

            var request = new CreateCourseRequest
            {
                Title = "New Course",
                Description = "Desc",
                CategoryId = cat.Id,
                CourseAccessType = CourseAccessType.SelfPaced
            };

            var result = await _courseService.CreateCourseAsync(user.Id, request);

            Assert.That(result.Title, Is.EqualTo("New Course"));
            Assert.That(result.Status, Is.EqualTo(CourseStatus.Draft));
        }

        [Test]
        public async Task UpdateCourseAsync_ValidUpdate_UpdatesCourse()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = new LMSApi.ModelLibrary.Models.Courses
            {
                Title = "Old",
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
                slug = "old",
                Status = CourseStatus.Draft
            };
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            var request = new UpdateCourseRequest { Title = "Updated", Description = "Desc" };

            var result = await _courseService.UpdateCourseAsync(course.Id, request);

            Assert.That(result.Title, Is.EqualTo("Updated"));
            Assert.That(result.Description, Is.EqualTo("Desc"));
        }

        [Test]
        public async Task PublishCourseAsync_SelfPaced_PublishesCourseAndSections()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = new LMSApi.ModelLibrary.Models.Courses
            {
                Title = "To Publish",
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
                slug = "to-publish",
                Status = CourseStatus.Draft,
                CourseAccessType = CourseAccessType.SelfPaced
            };
            var section = new CourseSection { Title = "S1", Description = "Desc", SectionId = 1, IsPublished = false };
            course.Sections.Add(section);
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            var result = await _courseService.PublishCourseAsync(course.Id);

            Assert.That(result.Status, Is.EqualTo(CourseStatus.Published));
            var dbSection = await DbContext.CourseSections.FindAsync(section.Id);
            Assert.That(dbSection!.IsPublished, Is.True);
        }

        [Test]
        public async Task PublishCourseAsync_CohortBasedWithoutBatches_ThrowsException()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = new LMSApi.ModelLibrary.Models.Courses
            {
                Title = "Cohort Course",
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
                slug = "cohort",
                Status = CourseStatus.Draft,
                CourseAccessType = CourseAccessType.CohortBased
            };
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() => _courseService.PublishCourseAsync(course.Id));
        }

        [Test]
        public async Task DeleteCourseAsync_WithEnrollments_ThrowsException()
        {
            var (user, cat) = await CreateUserAndCategory();
            var course = new LMSApi.ModelLibrary.Models.Courses
            {
                Title = "With Enrollments",
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
                slug = "with-enrollments"
            };
            DbContext.Courses.Add(course);
            var student = new Users { Email = "student@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Learner", Description = "Desc" } };
            DbContext.Users.Add(student);
            await DbContext.SaveChangesAsync();

            DbContext.Enrollments.Add(new Enrollments
            {
                CourseId = course.Id,
                UserId = student.Id,
                EnrolledAt = DateTime.UtcNow,
                ProgressPercentage = 0,
                IsCompleted = false
            });
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() => _courseService.DeleteCourseAsync(course.Id));
        }
    }
}
