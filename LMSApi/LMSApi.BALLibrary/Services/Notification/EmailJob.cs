using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Utils;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Configuration;

namespace LMSApi.BALLibrary.Services
{
    public class EmailJob : IEmailJob
    {
        private readonly IConfiguration _configuration;

        public EmailJob(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task ExecuteAsync(EmailMessage message)
        {
            await SendEmail.SendAsync(_configuration, message);
        }
    }
}
