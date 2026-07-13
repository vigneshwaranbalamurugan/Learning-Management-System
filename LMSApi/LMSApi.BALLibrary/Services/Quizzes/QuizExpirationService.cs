using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Interfaces.Quizzes;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LMSApi.BALLibrary.Services.Quizzes
{
    public class QuizExpirationService : IQuizExpirationService
    {
        private readonly IQuizAttemptRepository _attemptRepository;
        private readonly IStudentProgressService _progressService;
        private readonly ILogger<QuizExpirationService> _logger;

        public QuizExpirationService(
            IQuizAttemptRepository attemptRepository,
            IStudentProgressService progressService,
            ILogger<QuizExpirationService> logger)
        {
            _attemptRepository = attemptRepository;
            _progressService = progressService;
            _logger = logger;
        }

        public async Task ProcessExpiredQuizzesAsync()
        {
            _logger.LogInformation("Starting ProcessExpiredQuizzesAsync job.");

            try
            {
                var inProgressAttempts = await _attemptRepository.GetAllInProgressAttemptsAsync();
                
                foreach (var attempt in inProgressAttempts)
                {
                    if (attempt.Quiz == null) continue;

                    var timeElapsed = DateTime.UtcNow - attempt.StartedAt;
                    // Check if time limit exceeded with 10 mins buffer
                    if (attempt.Quiz.TimeLimit > TimeSpan.Zero && timeElapsed > attempt.Quiz.TimeLimit.Add(TimeSpan.FromMinutes(10)))
                    {
                        _logger.LogInformation($"Expiring quiz attempt {attempt.Id} for User {attempt.UserId}");

                        attempt.Status = AttemptStatus.Expired;
                        attempt.CompletedAt = DateTime.UtcNow;

                        // Calculate score based on any saved partial answers
                        double score = 0;
                        if (attempt.Answers != null && attempt.Answers.Any())
                        {
                            foreach (var answer in attempt.Answers)
                            {
                                if (answer.IsCorrect)
                                {
                                    var question = attempt.Quiz.Questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                                    if (question != null)
                                    {
                                        score += question.Mark;
                                    }
                                }
                            }
                        }

                        double totalMarks = attempt.Quiz.Questions.Sum(q => q.Mark);
                        attempt.Score = score;
                        attempt.IsPassed = totalMarks == 0 ? false : (score / totalMarks) * 100 >= attempt.Quiz.PassingPercentage;

                        await _attemptRepository.UpdateAsync(attempt);

                        // Recalculate Course progress based on submission reference
                        if (attempt.Quiz.CourseSection != null)
                        {
                            await _progressService.RecalculateCourseProgressAsync(attempt.UserId, attempt.Quiz.CourseSection.CourseId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing expired quizzes.");
            }

            _logger.LogInformation("Completed ProcessExpiredQuizzesAsync job.");
        }
    }
}
