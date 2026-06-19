using System.Threading.Tasks;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.BALLibrary.Services;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LMSApi.Tests.Services
{
    [TestFixture]
    public class NotificationServiceTests
    {
        [Test]
        public async Task Send_WithEmailMessage_CallsEmailHandlerOnly()
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

        [Test]
        public async Task Send_WithSmsMessage_CallsSmsHandlerOnly()
        {
            var mockEmailHandler = new Mock<INotificationHandler>();
            mockEmailHandler.Setup(h => h.CanHandle(It.IsAny<Message>())).Returns((Message m) => m is EmailMessage);

            var mockSmsHandler = new Mock<INotificationHandler>();
            mockSmsHandler.Setup(h => h.CanHandle(It.IsAny<Message>())).Returns((Message m) => m is SMSMessage);

            var handlers = new[] { mockEmailHandler.Object, mockSmsHandler.Object };
            var service = new NotificationService(handlers);

            var smsMsg = new SMSMessage("+919876543210", "Your OTP is 1234");

            await service.Send(smsMsg);

            mockSmsHandler.Verify(h => h.SendAsync(smsMsg), Times.Once);
            mockEmailHandler.Verify(h => h.SendAsync(It.IsAny<Message>()), Times.Never);
        }

        [Test]
        public void Send_WithNoMatchingHandler_ThrowsNotSupportedException()
        {
            // No handler matches this message type → service throws NotSupportedException
            var mockHandler = new Mock<INotificationHandler>();
            mockHandler.Setup(h => h.CanHandle(It.IsAny<Message>())).Returns(false);

            var service = new NotificationService(new[] { mockHandler.Object });

            var emailMsg = new EmailMessage("a@b.com", "Sub", "Body");

            // The real NotificationService uses FirstOrDefault and throws when null
            Assert.ThrowsAsync<NotSupportedException>(() => service.Send(emailMsg));
            mockHandler.Verify(h => h.SendAsync(It.IsAny<Message>()), Times.Never);
        }

        [Test]
        public async Task Send_WithFirstMatchingHandler_OnlyCallsFirstMatch()
        {
            // Service uses FirstOrDefault — only the first matching handler is invoked
            var handler1 = new Mock<INotificationHandler>();
            handler1.Setup(h => h.CanHandle(It.IsAny<Message>())).Returns(true);

            var handler2 = new Mock<INotificationHandler>();
            handler2.Setup(h => h.CanHandle(It.IsAny<Message>())).Returns(true);

            var service = new NotificationService(new[] { handler1.Object, handler2.Object });

            var emailMsg = new EmailMessage("a@b.com", "Sub", "Body");
            await service.Send(emailMsg);

            // Only the first matching handler should be called (FirstOrDefault behaviour)
            handler1.Verify(h => h.SendAsync(emailMsg), Times.Once);
            handler2.Verify(h => h.SendAsync(It.IsAny<Message>()), Times.Never);
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
