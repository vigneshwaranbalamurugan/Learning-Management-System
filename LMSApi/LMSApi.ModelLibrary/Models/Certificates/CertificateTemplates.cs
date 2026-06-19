namespace LMSApi.ModelLibrary.Models
{
    public class CertificateTemplates
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string TemplateBackgroundUrl { get; set; } = string.Empty; // Cloudinary URL to background image
        public int AspectRatioWidth { get; set; } = 16;   // e.g. 16 for 16:9
        public int AspectRatioHeight { get; set; } = 9;   // e.g. 9 for 16:9
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}