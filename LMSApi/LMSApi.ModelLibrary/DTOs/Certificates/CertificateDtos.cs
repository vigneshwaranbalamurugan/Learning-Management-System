using System.ComponentModel.DataAnnotations;

namespace LMSApi.ModelLibrary.DTOs
{
    // ─── Requests ─────────────────────────────────────────────────────────────

    public class CreateCertificateTemplateRequest
    {
        [Required(ErrorMessage = "Template name is required.")]
        [MaxLength(200, ErrorMessage = "Name must not exceed 200 characters.")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }


        /// <summary>Expected aspect ratio width component (default 16).</summary>
        public int AspectRatioWidth { get; set; } = 16;

        /// <summary>Expected aspect ratio height component (default 9).</summary>
        public int AspectRatioHeight { get; set; } = 9;
    }

    public class UpdateCertificateTemplateRequest
    {
        [MaxLength(200)]
        public string? Name { get; set; }
        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }

    // ─── Responses ────────────────────────────────────────────────────────────

    public class CertificateTemplateResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string TemplateBackgroundUrl { get; set; } = string.Empty;

        public int AspectRatioWidth { get; set; }
        public int AspectRatioHeight { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CertificateResponse
    {
        public int Id { get; set; }
        public Guid CertificateId { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string LearnerName { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public string CertificateImageUrl { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public string CourseDescription { get; set; } = string.Empty;
        public string CourseThumbnailUrl { get; set; } = string.Empty;
        public string CourseLevel { get; set; } = string.Empty;
        public double CourseDurationHours { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    public class CertificateVerificationResponse
    {
        public bool IsValid { get; set; }
        public CertificateResponse? Certificate { get; set; }
    }

    public class PagedCertificateTemplateResponse
    {
        public IEnumerable<CertificateTemplateResponse> Templates { get; set; } = [];
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class PagedCertificateResponse
    {
        public IEnumerable<CertificateResponse> Certificates { get; set; } = [];
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class CertificateRegenerationStatusResponse
    {
        public DateTime? LastRegeneratedAt { get; set; }
        public DateTime? NextAllowedAt { get; set; }
        public bool CanRegenerate { get; set; }
        public bool NameHasChanged { get; set; }
    }

    public class RegenerateCertificatesResponse
    {
        public int RegeneratedCount { get; set; }
        public DateTime? LastRegeneratedAt { get; set; }
        public DateTime? NextAllowedAt { get; set; }
    }
}
