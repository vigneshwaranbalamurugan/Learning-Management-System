using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Interfaces
{
    public interface ICertificateRepository
    {
        Task<Certificates?> GetByGuidAsync(Guid certificateId);
        Task<Certificates?> GetByUserAndCourseAsync(int userId, int courseId);
        Task<IEnumerable<Certificates>> GetCertificatesByUserAsync(int userId);
        Task<CertificateTemplates?> GetActiveTemplateAsync();
        Task<CertificateTemplates?> GetTemplateByIdAsync(int id);
        Task<IEnumerable<CertificateTemplates>> GetAllTemplatesAsync();
        Task AddCertificateAsync(Certificates certificate);
        Task UpdateCertificateAsync(Certificates certificate);
        Task AddTemplateAsync(CertificateTemplates template);
        Task UpdateTemplateAsync(CertificateTemplates template);
        Task DeactivateAllTemplatesAsync();
    }
}
