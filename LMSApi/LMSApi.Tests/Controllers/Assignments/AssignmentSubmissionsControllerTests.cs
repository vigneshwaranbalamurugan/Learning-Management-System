using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using LMSApi.API.Controllers;
using LMSApi.API.Handlers;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace LMSApi.Tests.Controllers.Assignments
{
    [TestFixture]
    public class AssignmentSubmissionsControllerTests
    {
        private Mock<IAssignmentSubmissionService> _mockSubmissionService;
        private Mock<IUploadService> _mockUploadService;
        private Mock<IConfiguration> _mockConfiguration;
        private AssignmentUploadHandler _uploadHandler;
        private AssignmentSubmissionsController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockSubmissionService = new Mock<IAssignmentSubmissionService>();
            _mockUploadService = new Mock<IUploadService>();
            _mockConfiguration = new Mock<IConfiguration>();

            _uploadHandler = new AssignmentUploadHandler(_mockUploadService.Object, _mockConfiguration.Object);
            _controller = new AssignmentSubmissionsController(_mockSubmissionService.Object, _uploadHandler);

            // Configure Mock User Principal
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "123") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Test]
        public void Submit_WhenAttachmentTypeIsFileAndAttachmentFileIsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var form = new AssignmentSubmissionFormRequest
            {
                AssignmentId = 1,
                AttachmentType = AssignmentSubmissonAttachmentType.File,
                AttachmentFile = null
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _controller.Submit(form));
            Assert.That(ex.Message, Is.EqualTo("Assignment attachment is required."));
        }

        [Test]
        public void Submit_WhenAttachmentTypeIsFileAndAttachmentFileIsEmpty_ThrowsInvalidOperationException()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);

            var form = new AssignmentSubmissionFormRequest
            {
                AssignmentId = 1,
                AttachmentType = AssignmentSubmissonAttachmentType.File,
                AttachmentFile = mockFile.Object
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _controller.Submit(form));
            Assert.That(ex.Message, Is.EqualTo("Assignment attachment is required."));
        }

        [Test]
        public void Submit_WhenAttachmentTypeIsFileAndAttachmentFileSizeExceeded_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockConfiguration.Setup(c => c["FileSizeLimits:AssignmentAttachmentInMB"]).Returns("10");

            var mockFile = new Mock<IFormFile>();
            // 11 MB = 11 * 1024 * 1024 bytes
            mockFile.Setup(f => f.Length).Returns(11 * 1024 * 1024);

            var form = new AssignmentSubmissionFormRequest
            {
                AssignmentId = 1,
                AttachmentType = AssignmentSubmissonAttachmentType.File,
                AttachmentFile = mockFile.Object
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _controller.Submit(form));
            Assert.That(ex.Message, Contains.Substring("Assignment attachment size exceeds the allowed limit"));
        }

        [Test]
        public void Submit_WhenAttachmentTypeIsFileAndFileTypeNotAllowed_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockConfiguration.Setup(c => c["FileSizeLimits:AssignmentAttachmentInMB"]).Returns("10");
            _mockUploadService.Setup(u => u.IsAllowedAssignmentAttachment(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.FileName).Returns("virus.exe");
            mockFile.Setup(f => f.ContentType).Returns("application/octet-stream");

            var form = new AssignmentSubmissionFormRequest
            {
                AssignmentId = 1,
                AttachmentType = AssignmentSubmissonAttachmentType.File,
                AttachmentFile = mockFile.Object
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await _controller.Submit(form));
            Assert.That(ex.Message, Contains.Substring("Invalid file type for assignment attachment"));
        }

        [Test]
        public async Task Submit_WhenAttachmentTypeIsFileAndFileIsValid_SubmitsSuccessfully()
        {
            // Arrange
            _mockConfiguration.Setup(c => c["FileSizeLimits:AssignmentAttachmentInMB"]).Returns("10");
            _mockUploadService.Setup(u => u.IsAllowedAssignmentAttachment("assignment.pdf", "application/pdf"))
                .Returns(true);

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.FileName).Returns("assignment.pdf");
            mockFile.Setup(f => f.ContentType).Returns("application/pdf");
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

            var form = new AssignmentSubmissionFormRequest
            {
                AssignmentId = 1,
                AttachmentType = AssignmentSubmissonAttachmentType.File,
                AttachmentFile = mockFile.Object,
                SubmissionText = "Text Submission",
                SubmittedAssignmentUrl = "http://uploaded.url"
            };

            var mockResponse = new AssignmentSubmissionResponse
            {
                Id = 1,
                AssignmentId = 1,
                StudentId = 123,
                Status = "Submitted"
            };

            _mockSubmissionService.Setup(s => s.SubmitAssignmentAsync(
                It.Is<int>(id => id == 123),
                It.Is<AssignmentSubmissionRequest>(r => r.AssignmentId == 1),
                It.IsAny<Stream>(),
                It.Is<string>(n => n == "assignment.pdf")
            )).ReturnsAsync(mockResponse);

            // Act
            var result = await _controller.Submit(form);

            // Assert
            Assert.That(result, Is.Not.Null);
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            var responseData = okResult.Value as AssignmentSubmissionResponse;
            Assert.That(responseData, Is.Not.Null);
            Assert.That(responseData.Id, Is.EqualTo(1));
            Assert.That(responseData.StudentId, Is.EqualTo(123));
        }

        [Test]
        public async Task Submit_WhenAttachmentTypeIsLinkAndAttachmentFileIsNull_SubmitsSuccessfully()
        {
            // Arrange
            var form = new AssignmentSubmissionFormRequest
            {
                AssignmentId = 1,
                AttachmentType = AssignmentSubmissonAttachmentType.Link,
                AttachmentFile = null,
                SubmissionText = "Text Submission",
                SubmittedAssignmentUrl = "http://submission.link"
            };

            var mockResponse = new AssignmentSubmissionResponse
            {
                Id = 2,
                AssignmentId = 1,
                StudentId = 123,
                Status = "Submitted"
            };

            _mockSubmissionService.Setup(s => s.SubmitAssignmentAsync(
                It.Is<int>(id => id == 123),
                It.Is<AssignmentSubmissionRequest>(r => r.AssignmentId == 1),
                It.Is<Stream>(s => s == null),
                It.Is<string>(n => n == null)
            )).ReturnsAsync(mockResponse);

            // Act
            var result = await _controller.Submit(form);

            // Assert
            Assert.That(result, Is.Not.Null);
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            var responseData = okResult.Value as AssignmentSubmissionResponse;
            Assert.That(responseData, Is.Not.Null);
            Assert.That(responseData.Id, Is.EqualTo(2));
            Assert.That(responseData.StudentId, Is.EqualTo(123));

            // Verify ValidateAssignmentAttachment is never called indirectly (no throw on null/empty file)
            _mockUploadService.Verify(u => u.IsAllowedAssignmentAttachment(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
