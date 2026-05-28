namespace LMSApi.ModelLibrary.Models
{
    public class Certificates
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int UserId { get; set; }
        public int CertificateNumber { get; set; }
        public int CertificateTemplateId { get; set; }
        public DateTime IssuedAt { get; set; }

        // Navigation properties
        public Courses Course { get; set; }
        public Users User { get; set; }
    }
}