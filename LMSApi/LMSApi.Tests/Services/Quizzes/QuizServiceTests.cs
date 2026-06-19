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
    public class QuizServiceTests : BaseServiceTest
    {
        private Mock<ILogger<QuizService>> _mockLogger = null!;
        private Mock<INotificationService> _mockNotificationService = null!;
        private IQuizService _quizService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _mockLogger = new Mock<ILogger<QuizService>>();
            _mockNotificationService = new Mock<INotificationService>();
            
            var quizRepository = new QuizRepository(DbContext);
            var sectionRepository = new CourseSectionRepository(DbContext);
            var courseRepository = new CourseRepository(DbContext);
            var enrollmentRepository = new EnrollmentRepository(DbContext);
            var batchRepo = new CourseBatchRepository(DbContext);

            _quizService = new QuizService(
                quizRepository,
                sectionRepository,
                enrollmentRepository,
                courseRepository,
                Mapper,
                _mockLogger.Object,
                _mockNotificationService.Object,
                batchRepo
            );
        }

        // ─── CreateQuizAsync ───────────────────────────────────────────────────

        [Test]
        public async Task CreateQuizAsync_ValidRequest_CreatesQuiz()
        {
            var (_, section, _) = await SetupCourseAndSection();
            var req = new CreateQuizRequest { Title = "Quiz1", CourseSectionId = section.Id };
            
            var res = await _quizService.CreateQuizAsync(req);

            Assert.That(res.Title, Is.EqualTo("Quiz1"));
        }

        [Test]
        public void CreateQuizAsync_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _quizService.CreateQuizAsync(null!));
        }

        [Test]
        public async Task CreateQuizAsync_EmptyTitle_ThrowsArgumentException()
        {
            var (_, section, _) = await SetupCourseAndSection();
            var req = new CreateQuizRequest { Title = "  ", CourseSectionId = section.Id };
            Assert.ThrowsAsync<ArgumentException>(() => _quizService.CreateQuizAsync(req));
        }

        // ─── PublishQuizAsync ──────────────────────────────────────────────────

        [Test]
        public async Task PublishQuizAsync_SelfPacedCourse_ThrowsInvalidOperationException()
        {
            var (_, section, _) = await SetupCourseAndSection(CourseAccessType.SelfPaced);
            var quiz = new Quzzes { Title = "Q", CourseSectionId = section.Id, Status = PublishStatus.Draft };
            DbContext.Quizzes.Add(quiz);
            await DbContext.SaveChangesAsync();

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _quizService.PublishQuizAsync(quiz.Id, new PublishQuizRequest { Publish = true }));
        }

        [Test]
        public async Task PublishQuizAsync_CohortBasedWithNoQuestions_ThrowsInvalidOperationException()
        {
            var (_, section, _) = await SetupCourseAndSection(CourseAccessType.CohortBased);
            var quiz = new Quzzes { Title = "Empty Quiz", CourseSectionId = section.Id, Status = PublishStatus.Draft };
            DbContext.Quizzes.Add(quiz);
            await DbContext.SaveChangesAsync();

            // No questions added → should throw
            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _quizService.PublishQuizAsync(quiz.Id, new PublishQuizRequest { Publish = true }));
        }

        [Test]
        public async Task PublishQuizAsync_CohortBased_PublishesQuiz()
        {
            var (_, section, _) = await SetupCourseAndSection(CourseAccessType.CohortBased);
            var quiz = new Quzzes { Title = "Q", CourseSectionId = section.Id, Status = PublishStatus.Draft };
            DbContext.Quizzes.Add(quiz);
            await DbContext.SaveChangesAsync(); // save quiz first so quiz.Id is set for FK
            // Add a question to allow publishing
            DbContext.QuizQuestions.Add(new QuizQuestions { QuizId = quiz.Id, QuestionText = "Q", Mark = 1, Explanation = "" });
            await DbContext.SaveChangesAsync();

            var result = await _quizService.PublishQuizAsync(quiz.Id, new PublishQuizRequest { Publish = true });
            Assert.That(result.Status, Is.EqualTo(PublishStatus.Published));
        }

        [Test]
        public async Task PublishQuizAsync_CohortBased_WithEnrolledLearners_SendsNotificationEmails()
        {
            var (_, section, course) = await SetupCourseAndSection(CourseAccessType.CohortBased);
            var quiz = new Quzzes { Title = "Q", CourseSectionId = section.Id, Status = PublishStatus.Draft };
            DbContext.Quizzes.Add(quiz);
            await DbContext.SaveChangesAsync(); // save quiz first so quiz.Id is set for FK
            DbContext.QuizQuestions.Add(new QuizQuestions { QuizId = quiz.Id, QuestionText = "Q", Mark = 1, Explanation = "" });

            var student = new Users { Email = "qstudent@test.com", PasswordHash="h", PasswordSalt="s", Role = new UserRoles { RoleName = "Learner", Description = "Desc" } };
            DbContext.Users.Add(student);
            await DbContext.SaveChangesAsync();

            DbContext.Enrollments.Add(new Enrollments
            {
                CourseId = course.Id, UserId = student.Id,
                EnrollmentStatus = EnrollmentStatus.Active, EnrolledAt = DateTime.UtcNow,
                ProgressPercentage = 0, IsCompleted = false
            });
            await DbContext.SaveChangesAsync();

            await _quizService.PublishQuizAsync(quiz.Id, new PublishQuizRequest { Publish = true });
            
            await Task.Delay(100);
            _mockNotificationService.Verify(x => x.Send(It.IsAny<Message>()), Times.Once);
        }

        [Test]
        public async Task PublishQuizAsync_CohortBased_NoEnrolledLearners_SendsNoEmail()
        {
            var (_, section, _) = await SetupCourseAndSection(CourseAccessType.CohortBased);
            var quiz = new Quzzes { Title = "Q", CourseSectionId = section.Id, Status = PublishStatus.Draft };
            DbContext.Quizzes.Add(quiz);
            await DbContext.SaveChangesAsync(); // save quiz first so quiz.Id is set for FK
            DbContext.QuizQuestions.Add(new QuizQuestions { QuizId = quiz.Id, QuestionText = "Q", Mark = 1, Explanation = "" });
            await DbContext.SaveChangesAsync();

            await _quizService.PublishQuizAsync(quiz.Id, new PublishQuizRequest { Publish = true });
            
            await Task.Delay(100);
            _mockNotificationService.Verify(x => x.Send(It.IsAny<Message>()), Times.Never);
        }

        // ─── Cohort Deadline Date and Validation Tests ────────────────────────

        [Test]
        public async Task CreateQuizAsync_CohortBased_DeadlineDateRequired_ThrowsArgumentException()
        {
            var (_, section, _) = await SetupCourseAndSection(CourseAccessType.CohortBased);
            var req = new CreateQuizRequest
            {
                CourseSectionId = section.Id,
                Title = "Cohort Quiz No Deadline",
                DeadlineDate = null
            };

            Assert.ThrowsAsync<ArgumentException>(() => _quizService.CreateQuizAsync(req));
        }

        [Test]
        public async Task CreateQuizAsync_CohortBased_DeadlineDateExceedsBatchEndDate_ThrowsArgumentException()
        {
            var (_, section, course) = await SetupCourseAndSection(CourseAccessType.CohortBased);
            
            var batch = new CourseBatch
            {
                CourseId = course.Id,
                Name = "Batch A",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(5),
                MaxStudents = 10,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            DbContext.CourseBatches.Add(batch);
            await DbContext.SaveChangesAsync();

            var req = new CreateQuizRequest
            {
                CourseSectionId = section.Id,
                Title = "Cohort Quiz Exceeds",
                DeadlineDate = DateTime.UtcNow.AddDays(6) // Exceeds 5 days
            };

            Assert.ThrowsAsync<ArgumentException>(() => _quizService.CreateQuizAsync(req));
        }

        [Test]
        public async Task CreateQuizAsync_CohortBased_ValidDeadlineDate_CreatesQuiz()
        {
            var (_, section, course) = await SetupCourseAndSection(CourseAccessType.CohortBased);
            
            var batch = new CourseBatch
            {
                CourseId = course.Id,
                Name = "Batch B",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(5),
                MaxStudents = 10,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            DbContext.CourseBatches.Add(batch);
            await DbContext.SaveChangesAsync();

            var req = new CreateQuizRequest
            {
                CourseSectionId = section.Id,
                Title = "Cohort Quiz Valid",
                DeadlineDate = DateTime.UtcNow.AddDays(4)
            };

            var result = await _quizService.CreateQuizAsync(req);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.DeadlineDate, Is.EqualTo(req.DeadlineDate));
        }

        [Test]
        public async Task UpdateQuizAsync_CohortBased_DeadlineDateExceedsBatchEndDate_ThrowsArgumentException()
        {
            var (_, section, course) = await SetupCourseAndSection(CourseAccessType.CohortBased);
            
            var batch = new CourseBatch
            {
                CourseId = course.Id,
                Name = "Batch C",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(5),
                MaxStudents = 10,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            DbContext.CourseBatches.Add(batch);
            await DbContext.SaveChangesAsync();

            var quiz = new Quzzes
            {
                Title = "Original Quiz",
                CourseSectionId = section.Id,
                DeadlineDate = DateTime.UtcNow.AddDays(2)
            };
            DbContext.Quizzes.Add(quiz);
            await DbContext.SaveChangesAsync();

            var req = new UpdateQuizRequest
            {
                DeadlineDate = DateTime.UtcNow.AddDays(10) // exceeds
            };

            Assert.ThrowsAsync<ArgumentException>(() => _quizService.UpdateQuizAsync(quiz.Id, req));
        }
    }
}
