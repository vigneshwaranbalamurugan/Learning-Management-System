using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class LessonResourceConfiguration : IEntityTypeConfiguration<LessonResources>
    {
        public void Configure(EntityTypeBuilder<LessonResources> builder)
        {
            builder.ToTable("LessonResources");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.ResourceTitle)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(r => r.ResourceUrl)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(r => r.Description)
                .HasMaxLength(1000);

            builder.Property(r => r.UploadedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(r => r.Lesson)
                .WithMany(l => l.Resources)
                .HasForeignKey(r => r.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
