using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Utils;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Configuration;

namespace LMSApi.BALLibrary.Services
{
    public class EmailNotificationHandler : INotificationHandler
    {
        private readonly IConfiguration _configuration;

        public EmailNotificationHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool CanHandle(Message message) => message is EmailMessage;

        public Task SendAsync(Message message)
        {
            if (message is not EmailMessage emailMessage)
            {
                throw new NotSupportedException("Unsupported notification message type for email handler.");
            }

            return SendEmail.SendAsync(_configuration, emailMessage);
        }
    }
}