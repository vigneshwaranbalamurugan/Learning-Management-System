using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Utils;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Configuration;
using Hangfire;

namespace LMSApi.BALLibrary.Services
{
    public class EmailNotificationHandler : INotificationHandler
    {
        private readonly IBackgroundJobClient _backgroundJobClient;

        public EmailNotificationHandler(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }

        public bool CanHandle(Message message) => message is EmailMessage;

        public Task SendAsync(Message message)
        {
            if (message is not EmailMessage emailMessage)
            {
                throw new NotSupportedException("Unsupported notification message type for email handler.");
            }

            _backgroundJobClient.Enqueue<IEmailJob>(job => job.ExecuteAsync(emailMessage));
            return Task.CompletedTask;
        }
    }
}