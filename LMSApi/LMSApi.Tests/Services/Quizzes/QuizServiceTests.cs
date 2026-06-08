using System;
using System.Linq;
using System.Threading.Tasks;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Services;
using LMSApi.DALLibrary.Repositories;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;
using LMSApi.ModelLibrary.Enums;
using Moq;
using NUnit.Framework;

namespace LMSApi.Tests.Services.Quizzes
{
    [TestFixture]
    public class QuizServiceTests : BaseServiceTest
    {
        private Mock<ILogger<QuizService>> _mockLogger = null!;
        private IQuizService _quizService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mockLogger = new Mock<ILogger<QuizService>>();
            
            var quizRepo = new QuizRepository(DbContext);
            var sectionRepo = new CourseSectionRepository(DbContext);
            var courseRepo = new CourseRepository(DbContext);
            var attemptRepo = new QuizAttemptRepository(DbContext);
            var answerRepo = new QuizAnswerRepository(DbContext);
            var enrollmentRepo = new EnrollmentRepository(DbContext);
            var progressService = new Mock<IStudentProgressService>();
            var questionRepo = new QuizQuestionRepository(DbContext);
            var optionRepo = new QuizOptionRepository(DbContext);

            _quizService = new QuizService(
                quizRepo,
                attemptRepo,
                questionRepo,
                optionRepo,
                answerRepo,
                sectionRepo,
                enrollmentRepo,
                courseRepo,
                progressService.Object,
                Mapper,
                _mockLogger.Object
            );
        }

        [Test]
        public async Task CreateQuizAsync_ValidRequest_CreatesQuiz()
        {
            var inst = new Users { Email = "inst@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Instructor", Description = "Desc" } };
            var cat = new CourseCategories { Name = "Cat", Description = "Desc" };
            DbContext.Users.Add(inst);
            DbContext.CourseCategories.Add(cat);
            await DbContext.SaveChangesAsync();

            var course = new LMSApi.ModelLibrary.Models.Courses { Title = "Course", Description = "Desc", Price = 0m, ThumbnailUrl = "url", IntroVideoUrl = "url", IsPremium = false, IsPublished = false, Requirements = "Reqs", LearningOutcomes = "Outcomes", EstimatedDuration = TimeSpan.Zero, Level = LMSApi.ModelLibrary.Enums.CourseLevel.Beginner, Language = LMSApi.ModelLibrary.Enums.CourseLanguage.English, PublishedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, DefaultAssignmentDeadlineDays = 7, CategoryId = cat.Id, InstructorId = inst.Id, slug = Guid.NewGuid().ToString() };
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            var section = new CourseSection { Title = "Sec", Description = "Desc", SectionId = 1, CourseId = course.Id };
            DbContext.CourseSections.Add(section);
            await DbContext.SaveChangesAsync();

            var req = new CreateQuizRequest { Title = "Quiz1", CourseSectionId = section.Id };
            
            var res = await _quizService.CreateQuizAsync(req);

            Assert.That(res.Title, Is.EqualTo("Quiz1"));
        }

        [Test]
        public async Task StartAttemptAsync_CreatesAttempt()
        {
            var inst = new Users { Email = "inst@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Instructor", Description = "Desc" } };
            var cat = new CourseCategories { Name = "Cat", Description = "Desc" };
            DbContext.Users.Add(inst);
            DbContext.CourseCategories.Add(cat);
            await DbContext.SaveChangesAsync();

            var course = new LMSApi.ModelLibrary.Models.Courses { Title = "Course", Description = "Desc", Price = 0m, ThumbnailUrl = "url", IntroVideoUrl = "url", IsPremium = false, IsPublished = false, Requirements = "Reqs", LearningOutcomes = "Outcomes", EstimatedDuration = TimeSpan.Zero, Level = LMSApi.ModelLibrary.Enums.CourseLevel.Beginner, Language = LMSApi.ModelLibrary.Enums.CourseLanguage.English, PublishedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, DefaultAssignmentDeadlineDays = 7, CategoryId = cat.Id, InstructorId = inst.Id, slug = Guid.NewGuid().ToString() };
            DbContext.Courses.Add(course);
            await DbContext.SaveChangesAsync();

            var section = new CourseSection { Title = "Sec", Description = "Desc", SectionId = 1, CourseId = course.Id };
            DbContext.CourseSections.Add(section);
            await DbContext.SaveChangesAsync();

            var user = new Users { Email = "s@s.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Learner", Description = "Desc" } };
            DbContext.Users.Add(user);
            await DbContext.SaveChangesAsync();

            var quiz = new Quzzes { Title = "Q1", CourseSectionId = section.Id, IsPublished = true };
            DbContext.Quizzes.Add(quiz);
            
            var enrollment = new Enrollments { CourseId = course.Id, UserId = user.Id, EnrollmentStatus = EnrollmentStatus.Active, EnrolledAt = DateTime.UtcNow, ProgressPercentage = 0, IsCompleted = false };
            DbContext.Enrollments.Add(enrollment);
            
            await DbContext.SaveChangesAsync();

            var attempt = await _quizService.StartAttemptAsync(quiz.Id, user.Id);

            Assert.That(attempt.QuizId, Is.EqualTo(quiz.Id));
            Assert.That(attempt.UserId, Is.EqualTo(user.Id));
        }
    }
}
