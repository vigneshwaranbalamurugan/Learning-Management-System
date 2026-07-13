using LMSApi.API.Extensions;
using LMSApi.BALLibrary.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LMSApi.API.Hubs
{
    [Authorize]
    public class QuizProgressHub : Hub
    {
        private readonly IQuizAttemptService _quizAttemptService;
        private readonly ILogger<QuizProgressHub> _logger;

        public QuizProgressHub(
            IQuizAttemptService quizAttemptService,
            ILogger<QuizProgressHub> logger)
        {
            _quizAttemptService = quizAttemptService;
            _logger = logger;
        }

        public async Task SavePartialAnswer(int attemptId, int questionId, int selectedOptionId)
        {
            var userId = Context.User!.GetUserId();
            try
            {
                await _quizAttemptService.SavePartialAnswerAsync(attemptId, questionId, selectedOptionId, userId);
                await Clients.Caller.SendAsync("AnswerSaved", new { QuestionId = questionId, SelectedOptionId = selectedOptionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving partial answer for User {UserId}, Attempt {AttemptId}, Question {QuestionId}", userId, attemptId, questionId);
                await Clients.Caller.SendAsync("Error", new { Message = "Failed to save partial answer." });
            }
        }
    }
}
