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
    public class QuizQuestionTests : BaseServiceTest
    {
        private IQuizQuestionService _quizQuestionService = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            var quizRepository = new QuizRepository(DbContext);
            var questionRepo = new QuizQuestionRepository(DbContext);
            var optionRepo = new QuizOptionRepository(DbContext);

            _quizQuestionService = new QuizQuestionService(
                quizRepository,
                questionRepo,
                optionRepo,
                Mapper,
                new Mock<ILogger<QuizQuestionService>>().Object
            );
        }

        private List<CreateQuizOptionRequest> ValidMcqOptions() => new List<CreateQuizOptionRequest>
        {
            new CreateQuizOptionRequest { OptionText = "Option A", IsCorrect = true },
            new CreateQuizOptionRequest { OptionText = "Option B", IsCorrect = false },
            new CreateQuizOptionRequest { OptionText = "Option C", IsCorrect = false }
        };

        private List<CreateQuizOptionRequest> ValidTrueFalseOptions() => new List<CreateQuizOptionRequest>
        {
            new CreateQuizOptionRequest { OptionText = "True", IsCorrect = true },
            new CreateQuizOptionRequest { OptionText = "False", IsCorrect = false }
        };

        // ─── AddQuestionAsync ──────────────────────────────────────────────────

        [Test]
        public void AddQuestionAsync_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _quizQuestionService.AddQuestionAsync(null!));
        }

        [Test]
        public async Task AddQuestionAsync_EmptyQuestionText_ThrowsArgumentException()
        {
            var (_, section, _) = await SetupCourseAndSection(CourseAccessType.SelfPaced, CourseStatus.Draft);
            var quiz = new Quzzes { Title = "Q", CourseSectionId = section.Id, Status = PublishStatus.Draft };
            DbContext.Quizzes.Add(quiz);
            await DbContext.SaveChangesAsync();

            var request = new CreateQuizQuestionRequest
            {
                QuizId = quiz.Id,
                QuestionText = "  ", // empty
                Mark = 5,
                QuestionType = QuestionType.MultipleChoice,
                Options = ValidMcqOptions()
            };
            Assert.ThrowsAsync<ArgumentException>(() => _quizQuestionService.AddQuestionAsync(request));
        }

        [Test]
        public async Task AddQuestionAsync_ZeroMark_ThrowsArgumentException()
        {
            var (_, section, _) = await SetupCourseAndSection(CourseAccessType.SelfPaced, CourseStatus.Draft);
            var quiz = new Quzzes { Title = "Q", CourseSectionId = section.Id, Status = PublishStatus.Draft };
            DbContext.Quizzes.Add(quiz);
            await DbContext.SaveChangesAsync();

            var request = new CreateQuizQuestionRequest
            {
                QuizId = quiz.Id,
                QuestionText = "What is 2+2?",
                Mark = 0, // invalid
                QuestionType = QuestionType.MultipleChoice,
                Options = ValidMcqOptions()
            };
            Assert.ThrowsAsync<ArgumentException>(() => _quizQuestionService.AddQuestionAsync(request));
        }

        [Test]
        public async Task AddQuestionAsync_LessThanTwoOptions_ThrowsArgumentException()
        {
            var (_, section, _) = await SetupCourseAndSection(CourseAccessType.SelfPaced, CourseStatus.Draft);
            var quiz = new Quzzes { Title = "Q", CourseSectionId = section.Id };
            DbContext.Quizzes.Add(quiz);
            await DbContext.SaveChangesAsync();

            var request = new CreateQuizQuestionRequest
            {
                QuizId = quiz.Id,
                QuestionText = "Only one option?",
                Mark = 5,
                QuestionType = QuestionType.MultipleChoice,
                Options = new List<CreateQuizOptionRequest>
                {
                    new CreateQuizOptionRequest { OptionText = "Only", IsCorrect = true }
                } // only 1 option
            };
            Assert.ThrowsAsync<ArgumentException>(() => _quizQuestionService.AddQuestionAsync(request));
        }

        [Test]
        public async Task AddQuestionAsync_MultipleChoiceWithMultipleCorrect_ThrowsArgumentException()
        {
            var (_, section, _) = await SetupCourseAndSection(CourseAccessType.SelfPaced, CourseStatus.Draft);
            var quiz = new Quzzes { Title = "Q", CourseSectionId = section.Id };
            DbContext.Quizzes.Add(quiz);
            await DbContext.SaveChangesAsync();

            var request = new CreateQuizQuestionRequest
            {
                QuizId = quiz.Id,
                QuestionText = "Pick one?",
                Mark = 5,
                QuestionType = QuestionType.MultipleChoice,
                Options = new List<CreateQuizOptionRequest>
                {
                    new CreateQuizOptionRequest { OptionText = "A", IsCorrect = true },
                    new CreateQuizOptionRequest { OptionText = "B", IsCorrect = true } // two correct = invalid
                }
            };
            Assert.ThrowsAsync<ArgumentException>(() => _quizQuestionService.AddQuestionAsync(request));
        }

        [Test]
        public async Task AddQuestionAsync_TrueFalseWithThreeOptions_ThrowsArgumentException()
        {
            var (_, section, _) = await SetupCourseAndSection(CourseAccessType.SelfPaced, CourseStatus.Draft);
            var quiz = new Quzzes { Title = "Q", CourseSectionId = section.Id };
            DbContext.Quizzes.Add(quiz);
            await DbContext.SaveChangesAsync();

            var request = new CreateQuizQuestionRequest
            {
                QuizId = quiz.Id,
                QuestionText = "True or False?",
                Mark = 5,
                QuestionType = QuestionType.TrueFalse,
                Options = new List<CreateQuizOptionRequest>
                {
                    new CreateQuizOptionRequest { OptionText = "True", IsCorrect = true },
                    new CreateQuizOptionRequest { OptionText = "False", IsCorrect = false },
                    new CreateQuizOptionRequest { OptionText = "Maybe", IsCorrect = false } // 3 options = invalid for TF
                }
            };
            Assert.ThrowsAsync<ArgumentException>(() => _quizQuestionService.AddQuestionAsync(request));
        }

        [Test]
        public async Task AddQuestionAsync_ValidMcq_AddsQuestion()
        {
            var (_, section, _) = await SetupCourseAndSection(CourseAccessType.SelfPaced, CourseStatus.Draft);
            var quiz = new Quzzes { Title = "MCQ Quiz", CourseSectionId = section.Id };
            DbContext.Quizzes.Add(quiz);
            await DbContext.SaveChangesAsync();

            var request = new CreateQuizQuestionRequest
            {
                QuizId = quiz.Id,
                QuestionText = "What is 2+2?",
                Explanation = string.Empty,
                Mark = 5,
                QuestionType = QuestionType.MultipleChoice,
                SortOrder = 1,
                Options = ValidMcqOptions()
            };
            var result = await _quizQuestionService.AddQuestionAsync(request);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.QuestionText, Is.EqualTo("What is 2+2?"));
            Assert.That(result.Options.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task AddQuestionAsync_ValidTrueFalse_AddsQuestion()
        {
            var (_, section, _) = await SetupCourseAndSection(CourseAccessType.SelfPaced, CourseStatus.Draft);
            var quiz = new Quzzes { Title = "TF Quiz", CourseSectionId = section.Id };
            DbContext.Quizzes.Add(quiz);
            await DbContext.SaveChangesAsync();

            var request = new CreateQuizQuestionRequest
            {
                QuizId = quiz.Id,
                QuestionText = "Is the sky blue?",
                Explanation = string.Empty,
                Mark = 2,
                QuestionType = QuestionType.TrueFalse,
                SortOrder = 1,
                Options = ValidTrueFalseOptions()
            };
            var result = await _quizQuestionService.AddQuestionAsync(request);

            Assert.That(result.Options.Count, Is.EqualTo(2));
        }
    }
}
