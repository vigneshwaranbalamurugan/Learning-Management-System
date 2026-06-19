using LMSApi.ModelLibrary.DTOs;

namespace LMSApi.BALLibrary.Interfaces
{
    public interface ICertificateService
    {
        /// <summary>
        /// Issues a certificate when a learner completes 100% of a course.
        /// Idempotent — if a certificate already exists for the user+course, returns it.
        /// </summary>
        Task<CertificateResponse> IssueCertificateAsync(int userId, int courseId);

        /// <summary>Public certificate lookup by the unique Guid.</summary>
        Task<CertificateVerificationResponse> VerifyCertificateAsync(Guid certificateId);

        /// <summary>Admin: create a new certificate template with a background image.</summary>
        Task<CertificateTemplateResponse> CreateTemplateAsync(
            CreateCertificateTemplateRequest request,
            Stream backgroundStream,
            string backgroundFileName);

        /// <summary>Admin: list all certificate templates.</summary>
        Task<IEnumerable<CertificateTemplateResponse>> GetTemplatesAsync();

        /// <summary>Admin: update template fields (no image re-upload).</summary>
        Task<CertificateTemplateResponse> UpdateTemplateAsync(int templateId, UpdateCertificateTemplateRequest request);
    }
}
