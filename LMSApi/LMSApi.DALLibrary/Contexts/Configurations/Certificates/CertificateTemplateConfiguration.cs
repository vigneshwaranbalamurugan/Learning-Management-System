using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class CertificateTemplateConfiguration : IEntityTypeConfiguration<CertificateTemplates>
    {
        public void Configure(EntityTypeBuilder<CertificateTemplates> builder)
        {
            builder.ToTable("CertificateTemplates");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(t => t.Description)
                .HasMaxLength(1000);

            builder.Property(t => t.TemplateBackgroundUrl)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(t => t.AspectRatioWidth)
                .HasDefaultValue(16);

            builder.Property(t => t.AspectRatioHeight)
                .HasDefaultValue(9);

            builder.Property(t => t.IsActive)
                .HasDefaultValue(true);

            builder.Property(t => t.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(t => t.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
