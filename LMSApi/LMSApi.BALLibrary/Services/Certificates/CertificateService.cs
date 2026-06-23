using AutoMapper;
using Hangfire;
using LMSApi.BALLibrary.Interfaces;
using LMSApi.DALLibrary.Interfaces;
using LMSApi.ModelLibrary.DTOs;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.Enums;
using Microsoft.Extensions.Logging;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.IO;
using System.Text.RegularExpressions;

namespace LMSApi.BALLibrary.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly ICertificateRepository _certificateRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUploadService _uploadService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CertificateService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUserNotificationsService _userNotificationsService;

        public CertificateService(
            ICertificateRepository certificateRepository,
            ICourseRepository courseRepository,
            IUserRepository userRepository,
            IUploadService uploadService,
            IBackgroundJobClient backgroundJobClient,
            IUserProfileRepository userProfileRepository,
            IMapper mapper,
            ILogger<CertificateService> logger,
            IHttpClientFactory httpClientFactory,
            IUserNotificationsService userNotificationsService)
        {
            _certificateRepository = certificateRepository;
            _courseRepository = courseRepository;
            _userRepository = userRepository;
            _uploadService = uploadService;
            _backgroundJobClient = backgroundJobClient;
            _userProfileRepository = userProfileRepository;
            _mapper = mapper;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _userNotificationsService = userNotificationsService;
        }

        public async Task<CertificateResponse> IssueCertificateAsync(int userId, int courseId)
        {
            // 1. Idempotency check
            var existingCert = await _certificateRepository.GetByUserAndCourseAsync(userId, courseId);
            if (existingCert != null)
            {
                return _mapper.Map<CertificateResponse>(existingCert);
            }

            // 2. Fetch required data
            var course = await _courseRepository.GetCourseWithDetailsAsync(courseId);
            if (course == null) throw new InvalidOperationException("Course not found.");
            
            var user = await _userRepository.GetByIdAsync(userId);
            var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
            if (user == null) throw new InvalidOperationException("User not found.");

            var instructor = await _userRepository.GetByIdAsync(course.InstructorId);
            var instructorProfile = await _userProfileRepository.GetByUserIdAsync(course.InstructorId);

            var template = await _certificateRepository.GetActiveTemplateAsync();
            if (template == null)
            {
                _logger.LogWarning("No active certificate template found. Skipping generation for user {UserId}, course {CourseId}.", userId, courseId);
                throw new InvalidOperationException("No active certificate template found.");
            }

            // 3. Prepare data for placeholders
            var learnerName = userProfile != null ? $"{userProfile.FirstName} {userProfile.LastName}".Trim() : user.Email.Split('@')[0];
            var instructorName = instructorProfile != null ? $"{instructorProfile.FirstName} {instructorProfile.LastName}".Trim() : instructor?.Email?.Split('@')[0] ?? "Instructor";
            var certificateId = Guid.NewGuid();
            var issuedDate = DateTime.UtcNow.ToString("MMMM dd, yyyy");

            // 4. Generate the pdf
            string cloudinaryUrl;
            using (var memoryStream = await GenerateCertificatePdfAsync(template, course.Title, learnerName, instructorName, certificateId.ToString(), issuedDate))
            {
                var fileName = $"cert_{certificateId}.pdf";
                cloudinaryUrl = await _uploadService.UploadCertificatePdfAsync(memoryStream, fileName, certificateId.ToString());
            }

            // 5. Persist record
            var newCert = new Certificates
            {
                CertificateId = certificateId,
                CourseId = courseId,
                UserId = userId,
                CertificateTemplateId = template.Id,
                CertificateImageUrl = cloudinaryUrl,
                IssuedAt = DateTime.UtcNow
            };

            await _certificateRepository.AddCertificateAsync(newCert);

            // 6. Queue email
            _backgroundJobClient.Enqueue<ICertificateEmailJob>(job => job.ExecuteAsync(userId, course.Title, cloudinaryUrl, certificateId));

            try
            {
                await _userNotificationsService.CreateAndSendNotificationAsync(
                    userId: userId,
                    title: "Certificate Issued",
                    message: $"Congratulations! Your certificate for '{course.Title}' has been issued.",
                    type: NotificationType.CertificateIssued,
                    redirectUrl: cloudinaryUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send certificate issued realtime notification to User {UserId}", userId);
            }

            _logger.LogInformation("Certificate {CertificateId} generated successfully for user {UserId} and course {CourseId}.", certificateId, userId, courseId);

            return new CertificateResponse
            {
                Id = newCert.Id,
                CertificateId = newCert.CertificateId,
                CourseId = courseId,
                CourseName = course.Title,
                UserId = userId,
                LearnerName = learnerName,
                InstructorName = instructorName,
                CertificateImageUrl = cloudinaryUrl,
                IssuedAt = newCert.IssuedAt
            };
        }

        public async Task<IEnumerable<CertificateResponse>> GetMyCertificatesAsync(int userId)
        {
            var certs = await _certificateRepository.GetCertificatesByUserAsync(userId);
            return _mapper.Map<IEnumerable<CertificateResponse>>(certs);
        }

        public async Task<CertificateVerificationResponse> VerifyCertificateAsync(Guid certificateId)
        {
            var cert = await _certificateRepository.GetByGuidAsync(certificateId);
            if (cert == null)
            {
                return new CertificateVerificationResponse { IsValid = false };
            }

            var courseName = cert.Course?.Title ?? "Unknown Course";
            var learnerName = cert.User?.UserProfile != null 
                ? $"{cert.User.UserProfile.FirstName} {cert.User.UserProfile.LastName}".Trim() 
                : cert.User?.Email?.Split('@')[0] ?? "Unknown Learner";
            var instructorName = cert.Course?.Instructor?.UserProfile != null
                ? $"{cert.Course.Instructor.UserProfile.FirstName} {cert.Course.Instructor.UserProfile.LastName}".Trim()
                : cert.Course?.Instructor?.Email?.Split('@')[0] ?? "Unknown Instructor";

            return new CertificateVerificationResponse
            {
                IsValid = true,
                Certificate = new CertificateResponse
                {
                    Id = cert.Id,
                    CertificateId = cert.CertificateId,
                    CourseId = cert.CourseId,
                    CourseName = courseName,
                    UserId = cert.UserId,
                    LearnerName = learnerName,
                    InstructorName = instructorName,
                    CertificateImageUrl = cert.CertificateImageUrl,
                    IssuedAt = cert.IssuedAt
                }
            };
        }

        public async Task<CertificateTemplateResponse> CreateTemplateAsync(CreateCertificateTemplateRequest request, Stream backgroundStream, string backgroundFileName)
        {
            // Upload background
            var publicId = $"template_{Guid.NewGuid()}";
            var bgUrl = await _uploadService.UploadCertificateTemplateBackgroundAsync(backgroundStream, backgroundFileName, publicId);

            var template = new CertificateTemplates
            {
                Name = request.Name,
                Description = request.Description,

                AspectRatioWidth = request.AspectRatioWidth,
                AspectRatioHeight = request.AspectRatioHeight,
                TemplateBackgroundUrl = bgUrl,
                IsActive = true
            };

            await _certificateRepository.AddTemplateAsync(template);
            return _mapper.Map<CertificateTemplateResponse>(template);
        }

        public async Task<IEnumerable<CertificateTemplateResponse>> GetTemplatesAsync()
        {
            var templates = await _certificateRepository.GetAllTemplatesAsync();
            return _mapper.Map<IEnumerable<CertificateTemplateResponse>>(templates);
        }

        public async Task<CertificateTemplateResponse> UpdateTemplateAsync(int templateId, UpdateCertificateTemplateRequest request)
        {
            var template = await _certificateRepository.GetTemplateByIdAsync(templateId);
            if (template == null) throw new KeyNotFoundException("Template not found.");

            if (request.Name != null) template.Name = request.Name;
            if (request.Description != null) template.Description = request.Description;

            if (request.IsActive.HasValue) template.IsActive = request.IsActive.Value;

            await _certificateRepository.UpdateTemplateAsync(template);
            return _mapper.Map<CertificateTemplateResponse>(template);
        }

        private async Task<Stream> GenerateCertificatePdfAsync(
            CertificateTemplates template,
            string courseName,
            string learnerName,
            string instructorName,
            string certificateId,
            string issuedDate)
        {
            var client = _httpClientFactory.CreateClient();
            var bgBytes = await client.GetByteArrayAsync(template.TemplateBackgroundUrl);

            using var bgStream = new MemoryStream(bgBytes);
            using var bgImage = XImage.FromStream(() => bgStream);

            var document = new PdfDocument();
            var page = document.AddPage();
            
            // Set page size to match image dimensions exactly
            page.Width = bgImage.PixelWidth;
            page.Height = bgImage.PixelHeight;

            var gfx = XGraphics.FromPdfPage(page);

            // Draw background image
            gfx.DrawImage(bgImage, 0, 0, page.Width, page.Height);

            // Scale font sizes based on a 1080p template height to prevent zooming issues
            double scale = page.Height / 1080.0;

            var titleFont = new XFont("Arial", 52 * scale, XFontStyle.Bold);
            var bodyFont = new XFont("Arial", 26 * scale, XFontStyle.Regular);
            var nameFont = FitPdfFontSize(gfx, learnerName, page.Width * 0.6, 72 * scale, XFontStyle.Bold);
            var courseFont = FitPdfFontSize(gfx, courseName, page.Width * 0.6, 42 * scale, XFontStyle.Bold);
            var metaFont = new XFont("Arial", 22 * scale, XFontStyle.Regular);

            var textColor = XBrushes.Black;
            var titleColor = XBrushes.DarkBlue;

            var centerFormat = new XStringFormat
            {
                Alignment = XStringAlignment.Center,
                LineAlignment = XLineAlignment.Center
            };

            // Draw text
            gfx.DrawString("CERTIFICATE OF COMPLETION", titleFont, titleColor,
                new XRect(0, page.Height * 0.18, page.Width, titleFont.Height), centerFormat);

            gfx.DrawString("This is to certify that", bodyFont, textColor,
                new XRect(0, page.Height * 0.30, page.Width, bodyFont.Height), centerFormat);

            gfx.DrawString(learnerName, nameFont, titleColor,
                new XRect(0, page.Height * 0.40, page.Width, nameFont.Height), centerFormat);

            gfx.DrawString("has successfully completed the course", bodyFont, textColor,
                new XRect(0, page.Height * 0.52, page.Width, bodyFont.Height), centerFormat);

            gfx.DrawString(courseName, courseFont, titleColor,
                new XRect(0, page.Height * 0.62, page.Width, courseFont.Height), centerFormat);

            // Instructor and Date
            gfx.DrawString($"Instructor: {instructorName}", metaFont, textColor,
                new XRect(0, page.Height * 0.78, page.Width, metaFont.Height), centerFormat);

            gfx.DrawString($"Issued: {issuedDate}", metaFont, textColor,
                new XRect(0, page.Height * 0.83, page.Width, metaFont.Height), centerFormat);

            gfx.DrawString($"Certificate ID: {certificateId}", metaFont, textColor,
                new XRect(0, page.Height * 0.88, page.Width, metaFont.Height), centerFormat);

            var memoryStream = new MemoryStream();
            document.Save(memoryStream, false);
            memoryStream.Position = 0;

            return memoryStream;
        }

        private XFont FitPdfFontSize(XGraphics gfx, string text, double maxWidth, double maxFontSize, XFontStyle style)
        {
            double currentSize = maxFontSize;
            var font = new XFont("Arial", currentSize, style);
            
            var size = gfx.MeasureString(text, font);
            
            while (size.Width > maxWidth && currentSize > 5)
            {
                currentSize -= 1;
                font = new XFont("Arial", currentSize, style);
                size = gfx.MeasureString(text, font);
            }
            
            return font;
        }
    }
}
