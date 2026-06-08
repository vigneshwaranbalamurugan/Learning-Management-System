using LMSApi.BALLibrary.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;

namespace LMSApi.API.Handlers
{
    public class AssignmentUploadHandler
    {
        private readonly IUploadService _uploadService;
        private readonly IConfiguration _configuration;

        public AssignmentUploadHandler(IUploadService uploadService, IConfiguration configuration)
        {
            _uploadService = uploadService;
            _configuration = configuration;
        }

        public void ValidateAssignmentAttachment(IFormFile file)
        {
            if (file is null || file.Length == 0)
                throw new InvalidOperationException("Assignment attachment is required.");

            int allowedSizeMB = _configuration["FileSizeLimits:AssignmentAttachmentInMB"] is string s ? int.Parse(s) : 10;
            if (file.Length > allowedSizeMB * 1024 * 1024)
                throw new InvalidOperationException($"Assignment attachment size exceeds the allowed limit. Only files up to {allowedSizeMB} MB are allowed.");

            if (!_uploadService.IsAllowedAssignmentAttachment(file.FileName, file.ContentType ?? string.Empty))
                throw new InvalidOperationException("Invalid file type for assignment attachment. Allowed types include PDF, DOC, DOCX, ZIP, JPG, JPEG, PNG, TXT.");
        }
    }
}
