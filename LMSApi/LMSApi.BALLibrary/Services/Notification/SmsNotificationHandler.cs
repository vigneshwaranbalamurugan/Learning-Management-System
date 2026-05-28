using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Utils;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.BALLibrary.Services
{
    public class SmsNotificationHandler : INotificationHandler
    {
        public bool CanHandle(Message message) => message is SMSMessage;

        public Task SendAsync(Message message)
        {
            if (message is not SMSMessage smsMessage)
            {
                throw new NotSupportedException("Unsupported notification message type for SMS handler.");
            }

            return SendSMS.SendAsync(smsMessage);
        }
    }
}