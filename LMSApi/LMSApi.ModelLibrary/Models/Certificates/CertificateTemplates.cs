namespace LMSApi.ModelLibrary.Models
{
    public class CertificateTemplates
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TemplateUrl { get; set; } // URL to the certificate template file
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}