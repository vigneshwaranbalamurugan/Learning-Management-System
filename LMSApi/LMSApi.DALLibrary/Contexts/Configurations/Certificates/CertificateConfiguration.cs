using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class CertificateConfiguration : IEntityTypeConfiguration<Certificates>
    {
        public void Configure(EntityTypeBuilder<Certificates> builder)
        {
            builder.ToTable("Certificates");

            builder.HasKey(c => c.Id);

            // Unique index on the public GUID — used for verification lookups
            builder.HasIndex(c => c.CertificateId)
                .IsUnique();

            // One certificate per user per course
            builder.HasIndex(c => new { c.UserId, c.CourseId })
                .IsUnique();

            builder.Property(c => c.CertificateId)
                .IsRequired()
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(c => c.CertificateImageUrl)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(c => c.IssuedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Course)
                .WithMany()
                .HasForeignKey(c => c.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Template)
                .WithMany()
                .HasForeignKey(c => c.CertificateTemplateId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
