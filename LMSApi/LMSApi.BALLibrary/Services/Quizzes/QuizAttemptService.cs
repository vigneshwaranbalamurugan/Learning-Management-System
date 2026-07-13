using AutoMapper;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.Enums;
using LMSApi.DALLibrary.Interfaces;
using Microsoft.Extensions.Logging;

namespace LMSApi.BALLibrary.Services
{

    public class QuizAttemptService : IQuizAttemptService
    {

        private readonly IQuizRepository _quizRepository;
        private readonly IQuizAttemptRepository _attemptRepository;
        private readonly IQuizAnswerRepository _answerRepository;
        private readonly ICourseSectionRepository _sectionRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IStudentProgressService _progressService;
        private readonly IMapper _mapper;
        private readonly ILogger<QuizAttemptService> _logger;
        private readonly IUserNotificationsService _userNotificationsService;

        public QuizAttemptService(
            IQuizRepository quizRepository,
            IQuizAttemptRepository attemptRepository,
            IQuizAnswerRepository answerRepository,
            ICourseSectionRepository sectionRepository,
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            IStudentProgressService progressService,
            IMapper mapper,
            ILogger<QuizAttemptService> logger,
            IUserNotificationsService userNotificationsService)
        {
            _quizRepository = quizRepository;
            _attemptRepository = attemptRepository;
            _answerRepository = answerRepository;
            _sectionRepository = sectionRepository;
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _progressService = progressService;
            _mapper = mapper;
            _logger = logger;
            _userNotificationsService = userNotificationsService;
        }

        // ─── Student Quiz-Taking ────────────────────────────────────────────

        public async Task<QuizStudentDetailResponse> GetQuizForStudentAsync(int quizId, int userId)
        {
            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(quizId);
            if (quiz.Status != PublishStatus.Published)
            {
                throw new InvalidOperationException("This quiz is not available.");
            }

            var activeAttempt = await _attemptRepository.GetInProgressAttemptAsync(quizId, userId);
            if (activeAttempt == null)
            {
                throw new InvalidOperationException("You must start a quiz attempt before retrieving the questions.");
            }

            return _mapper.Map<QuizStudentDetailResponse>(quiz);
        }

        public async Task<StartAttemptResponse> StartAttemptAsync(int quizId, int userId)
        {
            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(quizId);

            if (quiz.Status != PublishStatus.Published)
            {
                throw new InvalidOperationException("Cannot start an attempt on an unpublished quiz.");
            }

            // Verify Enrollment
            var section = await _sectionRepository.GetByIdAsync(quiz.CourseSectionId);
            var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(userId, section.CourseId);

            var isEnrolled = enrollment != null && 
                (enrollment.EnrollmentStatus == EnrollmentStatus.Active || enrollment.EnrollmentStatus == EnrollmentStatus.Completed);

            if (!isEnrolled)
            {
                throw new UnauthorizedAccessException("Student must be enrolled in the course to start this quiz.");
            }

            var course = await _courseRepository.GetByIdAsync(section.CourseId)
                ?? throw new KeyNotFoundException($"Course with id '{section.CourseId}' not found.");

            // Verify availability dates
            var now = DateTime.UtcNow;
            if (course.CourseAccessType == CourseAccessType.CohortBased)
            {
                if (quiz.DeadlineDate.HasValue && now > quiz.DeadlineDate.Value)
                {
                    throw new InvalidOperationException("This quiz is no longer available.");
                }
            }
            else
            {
                if (quiz.DeadlineInDays > 0)
                {
                    var deadline = enrollment!.EnrolledAt.AddDays(quiz.DeadlineInDays);
                    if (now > deadline)
                    {
                        throw new InvalidOperationException("This quiz is no longer available.");
                    }
                }
            }

            // Verify max attempts using PG function
            var remaining = await _attemptRepository.GetRemainingAttemptsAsync(quizId, userId);

            if (remaining <= 0)
            {
                throw new InvalidOperationException($"Maximum number of attempts ({quiz.MaxAttempts}) has been reached for this quiz.");
            }

            // Verify if already passed
            var previousAttempts = await _attemptRepository.GetAttemptsByQuizAndUserAsync(quizId, userId);
            if (previousAttempts.Any(a => a.IsPassed))
            {
                throw new InvalidOperationException("You have already passed this quiz and cannot take it again.");
            }

            // Create in-progress attempt
            var attempt = new QuizAttempts
            {
                QuizId = quizId,
                UserId = userId,
                Status = AttemptStatus.InProgress,
                StartedAt = DateTime.UtcNow,
                Score = 0.0,
                IsPassed = false
            };

            await _attemptRepository.AddAsync(attempt);

            _logger.LogInformation("Quiz Started: QuizId={QuizId}, UserId={UserId}, AttemptId={AttemptId}", quizId, userId, attempt.Id);

            return new StartAttemptResponse
            {
                AttemptId = attempt.Id,
                QuizId = quizId,
                UserId = userId,
                StartedAt = attempt.StartedAt,
                TimeLimit = quiz.TimeLimit
            };
        }

        public async Task SavePartialAnswerAsync(int attemptId, int questionId, int selectedOptionId, int userId)
        {
            var attempt = await _attemptRepository.GetAttemptWithAnswersAsync(attemptId);
            if (attempt == null || attempt.UserId != userId || attempt.Status != AttemptStatus.InProgress)
            {
                return;
            }

            var question = attempt.Quiz.Questions.FirstOrDefault(q => q.Id == questionId);
            if (question == null) return;

            var selectedOption = question.Answers.FirstOrDefault(o => o.Id == selectedOptionId);
            if (selectedOption == null) return;

            var existingAnswer = attempt.Answers.FirstOrDefault(a => a.QuestionId == questionId);
            
            if (existingAnswer != null)
            {
                var updatedAnswer = new QuizAnswers
                {
                    Id = existingAnswer.Id,
                    AttemptId = existingAnswer.AttemptId,
                    QuestionId = existingAnswer.QuestionId,
                    SelectedOptionId = selectedOptionId,
                    IsCorrect = selectedOption.IsCorrect
                };
                await _answerRepository.UpdateAsync(updatedAnswer);
            }
            else
            {
                await _answerRepository.AddAsync(new QuizAnswers
                {
                    AttemptId = attemptId,
                    QuestionId = questionId,
                    SelectedOptionId = selectedOptionId,
                    IsCorrect = selectedOption.IsCorrect
                });
            }
        }

        public async Task<QuizAttemptResponse> SubmitQuizAsync(int quizId, int userId, SubmitQuizRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var quiz = await _quizRepository.GetQuizWithQuestionsAsync(quizId);

            // Find in-progress attempt
            var attempt = await _attemptRepository.GetInProgressAttemptAsync(quizId, userId)
                ?? throw new InvalidOperationException("No active in-progress attempt found for this quiz.");

            // Verify active status / time limit
            var timeElapsed = DateTime.UtcNow - attempt.StartedAt;
            if (quiz.TimeLimit > TimeSpan.Zero && timeElapsed > quiz.TimeLimit.Add(TimeSpan.FromSeconds(30)))
            {
                attempt.Status = AttemptStatus.Expired;
                attempt.CompletedAt = DateTime.UtcNow;
                await _attemptRepository.UpdateAsync(attempt);
                throw new InvalidOperationException("This quiz attempt has expired because the time limit was exceeded.");
            }

            var now = DateTime.UtcNow;
            
            var section = await _sectionRepository.GetByIdAsync(quiz.CourseSectionId);
            var enrollment = await _enrollmentRepository.GetByUserAndCourseAsync(userId, section.CourseId);
            
            var course = await _courseRepository.GetByIdAsync(section.CourseId)
                ?? throw new KeyNotFoundException($"Course with id '{section.CourseId}' not found.");

            if (course.CourseAccessType == CourseAccessType.CohortBased)
            {
                if (quiz.DeadlineDate.HasValue && now > quiz.DeadlineDate.Value)
                {
                    attempt.Status = AttemptStatus.Expired;
                    attempt.CompletedAt = DateTime.UtcNow;
                    await _attemptRepository.UpdateAsync(attempt);
                    throw new InvalidOperationException("This quiz is no longer available.");
                }
            }
            else
            {
                if (enrollment != null && quiz.DeadlineInDays > 0)
                {
                    var deadline = enrollment.EnrolledAt.AddDays(quiz.DeadlineInDays);
                    if (now > deadline)
                    {
                        attempt.Status = AttemptStatus.Expired;
                        attempt.CompletedAt = DateTime.UtcNow;
                        await _attemptRepository.UpdateAsync(attempt);
                        throw new InvalidOperationException("This quiz is no longer available.");
                    }
                }
            }

            // Clear existing partial answers to avoid duplication
            if (attempt.Answers != null && attempt.Answers.Any())
            {
                var answersToDelete = attempt.Answers.ToList();
                foreach (var existingAns in answersToDelete)
                {
                    await _answerRepository.DeleteAsync(existingAns.Id);
                }
            }

            // Save answers
            var answers = new List<QuizAnswers>();
            double score = 0;
            foreach (var answer in request.Answers)
            {
                var question = quiz.Questions.FirstOrDefault(q => q.Id == answer.QuestionId)
                    ?? throw new ArgumentException($"Question with id '{answer.QuestionId}' does not belong to quiz '{quizId}'.");

                var selectedOption = question.Answers.FirstOrDefault(o => o.Id == answer.SelectedOptionId)
                    ?? throw new ArgumentException($"Option with id '{answer.SelectedOptionId}' does not belong to question '{answer.QuestionId}'.");

                if (selectedOption.IsCorrect)
                {
                    score += question.Mark;
                }

                answers.Add(new QuizAnswers
                {
                    AttemptId = attempt.Id,
                    QuestionId = answer.QuestionId,
                    SelectedOptionId = answer.SelectedOptionId,
                    IsCorrect = selectedOption.IsCorrect
                });
            }

            await _answerRepository.AddRangeAsync(answers);

            attempt.Status = AttemptStatus.Submitted;
            attempt.CompletedAt = DateTime.UtcNow;

            double totalMarks = quiz.Questions.Sum(q => q.Mark);
            attempt.Score = score;
            attempt.IsPassed = totalMarks == 0 ? false : (score / totalMarks) * 100 >= quiz.PassingPercentage;
            attempt.Quiz = quiz;

            await _attemptRepository.UpdateAsync(attempt);

            _logger.LogInformation("Quiz Submitted: QuizId={QuizId}, UserId={UserId}, Score={Score}, Passed={Passed}",
                quizId, userId, attempt.Score, attempt.IsPassed);

            // Recalculate Course progress
             section = await _sectionRepository.GetByIdAsync(quiz.CourseSectionId);
            if (section != null)
            {
                await _progressService.RecalculateCourseProgressAsync(userId, section.CourseId);
            }

            try
            {
                var resultText = attempt.IsPassed ? "Passed ✅" : "Failed ❌";
                await _userNotificationsService.CreateAndSendNotificationAsync(
                    userId: userId,
                    title: $"Quiz Completed: {quiz.Title}",
                    message: $"You completed '{quiz.Title}' with a score of {attempt.Score}/{totalMarks} — {resultText}.",
                    type: NotificationType.QuizResult,
                    redirectUrl: $"/courses/{section.CourseId}/quizzes/{quiz.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send quiz result realtime notification to User {UserId}", userId);
            }

            return _mapper.Map<QuizAttemptResponse>(attempt);
        }

        public async Task<IEnumerable<QuizAttemptResponse>> GetUserAttemptsAsync(int quizId, int userId)
        {
            var attempts = await _attemptRepository.GetAttemptsByQuizAndUserAsync(quizId, userId);
            return _mapper.Map<IEnumerable<QuizAttemptResponse>>(attempts);
        }

        public async Task<QuizAttemptDetailResponse> GetAttemptDetailAsync(int attemptId)
        {
            var attempt = await _attemptRepository.GetAttemptWithAnswersAsync(attemptId);
            return _mapper.Map<QuizAttemptDetailResponse>(attempt);
        }

        public async Task<GetRemainingAttemptsResponse> GetRemainingAttemptsAsync(int quizId, int userId)
        {
            var quiz = await _quizRepository.GetByIdAsync(quizId);
            var remaining = await _attemptRepository.GetRemainingAttemptsAsync(quizId, userId);

            return new GetRemainingAttemptsResponse
            {
                QuizId = quizId,
                RemainingAttempts = remaining,
                MaxAttempts = quiz.MaxAttempts
            };
        }

        public async Task<IEnumerable<QuizAttemptResponse>> GetMyAttemptsAsync(int userId)
        {
            var attempts = await _attemptRepository.GetAttemptsByUserAsync(userId);
            return _mapper.Map<IEnumerable<QuizAttemptResponse>>(attempts);
        }

        public async Task<PagedQuizAttemptResponse> GetMyAttemptsPagedAsync(int userId, int pageNumber, int pageSize)
        {
            var (attempts, totalCount) = await _attemptRepository.GetAttemptsByUserPagedAsync(userId, pageNumber, pageSize);
            
            var attemptResponses = _mapper.Map<IEnumerable<QuizAttemptResponse>>(attempts);

            return new PagedQuizAttemptResponse
            {
                Attempts = attemptResponses,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }
   
    }
}