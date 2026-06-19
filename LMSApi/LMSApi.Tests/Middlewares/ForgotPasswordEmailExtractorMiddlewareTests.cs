using System.IO;
using System.Text;
using System.Threading.Tasks;
using LMSApi.API.Middlewares;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;

namespace LMSApi.Tests.Middlewares
{
    [TestFixture]
    public class ForgotPasswordEmailExtractorMiddlewareTests
    {
        [Test]
        public async Task InvokeAsync_ForgotPasswordPostRequest_ExtractsEmailAndResetsBodyPosition()
        {
            // Arrange
            var middleware = new ForgotPasswordEmailExtractorMiddleware(context => Task.CompletedTask);
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/v1/auth/forgot-password";
            context.Request.Method = "POST";
            
            var jsonString = "{\"email\":\"  test@example.com  \"}";
            var byteData = Encoding.UTF8.GetBytes(jsonString);
            var memoryStream = new MemoryStream(byteData);
            context.Request.Body = memoryStream;

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.That(context.Items.ContainsKey("ForgotPasswordEmail"), Is.True);
            Assert.That(context.Items["ForgotPasswordEmail"], Is.EqualTo("test@example.com"));
            Assert.That(context.Request.Body.Position, Is.EqualTo(0));
        }

        [Test]
        public async Task InvokeAsync_NonForgotPasswordRequest_DoesNotExtractEmail()
        {
            // Arrange
            var middleware = new ForgotPasswordEmailExtractorMiddleware(context => Task.CompletedTask);
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/v1/auth/login";
            context.Request.Method = "POST";
            
            var jsonString = "{\"email\":\"test@example.com\"}";
            var byteData = Encoding.UTF8.GetBytes(jsonString);
            var memoryStream = new MemoryStream(byteData);
            context.Request.Body = memoryStream;

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.That(context.Items.ContainsKey("ForgotPasswordEmail"), Is.False);
        }

        [Test]
        public async Task InvokeAsync_HttpGetRequest_DoesNotExtractEmail()
        {
            // Arrange
            var middleware = new ForgotPasswordEmailExtractorMiddleware(context => Task.CompletedTask);
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/v1/auth/forgot-password";
            context.Request.Method = "GET";

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.That(context.Items.ContainsKey("ForgotPasswordEmail"), Is.False);
        }

        [Test]
        public async Task InvokeAsync_InvalidJsonBody_GracefullyProceedsWithoutException()
        {
            // Arrange
            var middleware = new ForgotPasswordEmailExtractorMiddleware(context => Task.CompletedTask);
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/v1/auth/forgot-password";
            context.Request.Method = "POST";
            
            var invalidJson = "invalid json";
            var byteData = Encoding.UTF8.GetBytes(invalidJson);
            var memoryStream = new MemoryStream(byteData);
            context.Request.Body = memoryStream;

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await middleware.InvokeAsync(context));
            Assert.That(context.Items.ContainsKey("ForgotPasswordEmail"), Is.False);
        }
    }
}
