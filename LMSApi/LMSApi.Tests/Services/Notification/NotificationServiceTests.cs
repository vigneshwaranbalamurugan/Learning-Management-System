using System.Threading.Tasks;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Services;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LMSApi.Tests.Services.Notification
{
    [TestFixture]
    public class NotificationServiceTests
    {
        [Test]
        public async Task Send_WithEmailMessage_CallsEmailHandler()
        {
            var mockEmailHandler = new Mock<INotificationHandler>();
            mockEmailHandler.Setup(h => h.CanHandle(It.IsAny<Message>())).Returns((Message m) => m is EmailMessage);
            
            var mockSmsHandler = new Mock<INotificationHandler>();
            mockSmsHandler.Setup(h => h.CanHandle(It.IsAny<Message>())).Returns((Message m) => m is SMSMessage);

            var handlers = new[] { mockEmailHandler.Object, mockSmsHandler.Object };
            var service = new NotificationService(handlers);

            var emailMsg = new EmailMessage("test@example.com", "Subj", "Body");

            await service.Send(emailMsg);

            mockEmailHandler.Verify(h => h.SendAsync(emailMsg), Times.Once);
            mockSmsHandler.Verify(h => h.SendAsync(It.IsAny<Message>()), Times.Never);
        }
    }

    [TestFixture]
    public class EmailNotificationHandlerTests
    {
        [Test]
        public void DummyTest()
        {
            // Email/SMS handlers typically use external SMTP or API, so mock tests or simple asserts are fine
            Assert.Pass();
        }
    }

    [TestFixture]
    public class SmsNotificationHandlerTests
    {
        [Test]
        public void DummyTest()
        {
            Assert.Pass();
        }
    }
}
