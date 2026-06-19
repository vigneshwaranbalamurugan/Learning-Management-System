using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Services;
using LMSApi.DALLibrary.Repositories;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;
using LMSApi.ModelLibrary.Enums;
using Moq;
using NUnit.Framework;

namespace LMSApi.Tests.Services
{
    [TestFixture]
    public class QuizAttemptTests : BaseServiceTest
    {
        private Mock<INotificationService> _mockNotificationService = null!;
        private IQuizAttemptService _quizAttemptService = null!;
        private IQuizQuestionService _quizQuestionService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mockNotificationService = new Mock<INotificationService>();
            
            var quizRepository = new QuizRepository(DbContext);
            var sectionRepository = new CourseSectionRepository(DbContext);
            var courseRepository = new CourseRepository(DbContext);
            var enrollmentRepository = new EnrollmentRepository(DbContext);
            var attemptRepo = new QuizAttemptRepository(DbContext);
            var questionRepo = new QuizQuestionRepository(DbContext);
            var optionRepo = new QuizOptionRepository(DbContext);
            var answerRepo = new QuizAnswerRepository(DbContext);
            var progressService = new Mock<IStudentProgressService>();
            var batchRepo = new CourseBatchRepository(DbContext);

            _quizAttemptService = new QuizAttemptService(
                quizRepository,
                attemptRepo,
                answerRepo,
                sectionRepository,
                enrollmentRepository,
                courseRepository,
                progressService.Object,
                Mapper,
                new Mock<ILogger<QuizAttemptService>>().Object
            );

            _quizQuestionService = new QuizQuestionService(
                quizRepository,
                questionRepo,
                optionRepo,
                Mapper,
                new Mock<ILogger<QuizQuestionService>>().Object
            );
        }

        private async Task<Quzzes> CreatePublishedQuiz(int sectionId)
        {
            var quiz = new Quzzes { Title = "Quiz", CourseSectionId = sectionId, Status = PublishStatus.Published };
            DbContext.Quizzes.Add(quiz);
            await DbContext.SaveChangesAsync();
            return quiz;
        }

        private List<CreateQuizOptionRequest> ValidMcqOptions() => new List<CreateQuizOptionRequest>
        {
            new CreateQuizOptionRequest { OptionText = "Option A", IsCorrect = true },
            new CreateQuizOptionRequest { OptionText = "Option B", IsCorrect = false },
            new CreateQuizOptionRequest { OptionText = "Option C", IsCorrect = false }
        };

        // ─── StartAttemptAsync ─────────────────────────────────────────────────

        [Test]
        public async Task StartAttemptAsync_ValidEnrolledStudent_CreatesAttempt()
        {
            var (_, section, course) = await SetupCourseAndSection(CourseAccessType.SelfPaced, CourseStatus.Draft);
            var student = new Users { Email = "s@s.com", PasswordHash = "h", PasswordSalt = "s", Role = new UserRoles { RoleName = "Learner", Description = "Desc" } };
            DbContext.Users.Add(student);
            await DbContext.SaveChangesAsync();

            var quiz = await CreatePublishedQuiz(section.Id);

            var enrollment = new Enrollments { CourseId = course.Id, UserId = student.Id, EnrollmentStatus = EnrollmentStatus.Active, EnrolledAt = DateTime.UtcNow, ProgressPercentage = 0, IsCompleted = false };
            DbContext.Enrollments.Add(enrollment);
            await DbContext.SaveChangesAsync();

            var attempt = await _quizAttemptService.StartAttemptAsync(quiz.Id, student.Id);

            Assert.That(attempt.QuizId, Is.EqualTo(quiz.Id));
            Assert.That(attempt.UserId, Is.EqualTo(student.Id));
        }

        [Test]
        public async Task StartAttemptAsync_QuizNotPublished_ThrowsInvalidOperationException()
        {
            var (_, section, course) = await SetupCourseAndSection(CourseAccessType.SelfPaced, CourseStatus.Draft);
            var student = new Users { Email = $"s-{Guid.NewGuid()}@s.com", PasswordHash = "h", PasswordSalt = "s", Role = new UserRoles { RoleName = "Learner", Description = "Desc" } };
            DbContext.Users.Add(student);
            await DbContext.SaveChangesAsync();

            var quiz = new Quzzes { Title = "Draft Quiz", CourseSectionId = section.Id, Status = PublishStatus.Draft };
            DbContext.Quizzes.Add(quiz);
            var enrollment = new Enrollments { CourseId = course.Id, UserId = student.Id, EnrollmentStatus = EnrollmentStatus.Active, EnrolledAt = DateTime.UtcNow, ProgressPercentage = 0, IsCompleted = false };
            DbContext.Enrollments.Add(enrollment);
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() => _quizAttemptService.StartAttemptAsync(quiz.Id, student.Id));
        }

        [Test]
        public async Task StartAttemptAsync_StudentNotEnrolled_ThrowsUnauthorizedAccessException()
        {
            var (_, section, _) = await SetupCourseAndSection(CourseAccessType.SelfPaced, CourseStatus.Draft);
            var stranger = new Users { Email = "stranger@s.com", PasswordHash = "h", PasswordSalt = "s", Role = new UserRoles { RoleName = "Learner", Description = "Desc" } };
            DbContext.Users.Add(stranger);
            await DbContext.SaveChangesAsync();

            var quiz = await CreatePublishedQuiz(section.Id);

            Assert.ThrowsAsync<UnauthorizedAccessException>(() => _quizAttemptService.StartAttemptAsync(quiz.Id, stranger.Id));
        }

        [Test]
        public async Task SubmitQuizAsync_ValidSubmit_ReturnsTotalScoreAndObtainedScore()
        {
            var (_, section, course) = await SetupCourseAndSection(CourseAccessType.SelfPaced, CourseStatus.Draft);
            var student = new Users { Email = "student_test@s.com", PasswordHash = "h", PasswordSalt = "s", Role = new UserRoles { RoleName = "Learner", Description = "Desc" } };
            DbContext.Users.Add(student);
            await DbContext.SaveChangesAsync();

            var enrollment = new Enrollments { CourseId = course.Id, UserId = student.Id, EnrollmentStatus = EnrollmentStatus.Active, EnrolledAt = DateTime.UtcNow, ProgressPercentage = 0, IsCompleted = false };
            DbContext.Enrollments.Add(enrollment);
            await DbContext.SaveChangesAsync();

            var quiz = await CreatePublishedQuiz(section.Id);

            // Add a question
            var questionRequest = new CreateQuizQuestionRequest
            {
                QuizId = quiz.Id,
                QuestionText = "What is 2+2?",
                Explanation = string.Empty,
                Mark = 5,
                QuestionType = QuestionType.MultipleChoice,
                SortOrder = 1,
                Options = ValidMcqOptions()
            };
            var question = await _quizQuestionService.AddQuestionAsync(questionRequest);

            // Start attempt
            var attempt = await _quizAttemptService.StartAttemptAsync(quiz.Id, student.Id);

            // Submit attempt
            var submitRequest = new SubmitQuizRequest
            {
                Answers = new List<SubmitAnswerItem>
                {
                    new SubmitAnswerItem
                    {
                        QuestionId = question.Id,
                        SelectedOptionId = question.Options.First(o => o.IsCorrect).Id
                    }
                }
            };
            var response = await _quizAttemptService.SubmitQuizAsync(quiz.Id, student.Id, submitRequest);

            Assert.That(response.TotalScore, Is.EqualTo(5.0));
            Assert.That(response.ObtainedScore, Is.EqualTo(5.0));
            Assert.That(response.IsPassed, Is.True);
        }

        [Test]
        public async Task StartAttemptAsync_CohortBased_AfterDeadlineDate_ThrowsInvalidOperationException()
        {
            var (_, section, course) = await SetupCourseAndSection(CourseAccessType.CohortBased);

            var quiz = new Quzzes
            {
                Title = "Past Deadline Quiz",
                CourseSectionId = section.Id,
                DeadlineDate = DateTime.UtcNow.AddDays(-2), // past
                Status = PublishStatus.Published,
                MaxAttempts = 2
            };
            DbContext.Quizzes.Add(quiz);

            var student = new Users { Email = "latudent_quiz@test.com", PasswordHash = "h", PasswordSalt = "s", Role = new UserRoles { RoleName = "Learner", Description = "Desc" } };
            DbContext.Users.Add(student);
            await DbContext.SaveChangesAsync();

            DbContext.Enrollments.Add(new Enrollments
            {
                CourseId = course.Id,
                UserId = student.Id,
                EnrollmentStatus = EnrollmentStatus.Active,
                EnrolledAt = DateTime.UtcNow.AddDays(-5),
                ProgressPercentage = 0,
                IsCompleted = false
            });
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() => _quizAttemptService.StartAttemptAsync(quiz.Id, student.Id));
        }
    }
}
