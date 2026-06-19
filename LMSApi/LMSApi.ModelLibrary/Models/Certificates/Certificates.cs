namespace LMSApi.ModelLibrary.Models
{
    public class Certificates
    {
        public int Id { get; set; }
        public Guid CertificateId { get; set; } = Guid.NewGuid();
        public int CourseId { get; set; }
        public int UserId { get; set; }
        public int CertificateTemplateId { get; set; }
        public string CertificateImageUrl { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }

        // Navigation properties
        public Courses Course { get; set; } = null!;
        public Users User { get; set; } = null!;
        public CertificateTemplates Template { get; set; } = null!;
    }
}