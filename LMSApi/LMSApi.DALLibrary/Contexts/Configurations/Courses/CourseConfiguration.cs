using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.Enums;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class CourseConfiguration : IEntityTypeConfiguration<Courses>
    {
        public void Configure(EntityTypeBuilder<Courses> builder)
        {
            builder.ToTable("Courses");

            builder.HasKey(c => c.Id);

            builder.HasIndex(c => c.slug).IsUnique();

            builder.Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(c => c.slug)
                .IsRequired()
                .HasMaxLength(350);

            builder.Property(c => c.Description)
                .HasMaxLength(2000);

            builder.Property(c => c.Price)
                .HasColumnType("decimal(18,2)");

            builder.Property(c => c.ThumbnailUrl)
                .HasMaxLength(1000);

            builder.Property(c => c.IntroVideoUrl)
                .HasMaxLength(1000);

            builder.Property(c => c.Status)
                .HasDefaultValue(CourseStatus.Draft);

            builder.Property(c=>c.Level)
                .HasDefaultValue(CourseLevel.Beginner);

            builder.Property(c => c.LanguageId)
                .HasDefaultValue(1);

            builder.Property(c => c.IsPremium)
                .HasDefaultValue(false);

            builder.Property(c => c.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(c => c.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // ─── Hybrid Learning columns ──────────────────────────────────────
            builder.Property(c => c.CourseAccessType)
                .HasDefaultValue(CourseAccessType.SelfPaced);

            builder.Property(c => c.DefaultDeadlineDays)
                .IsRequired(false);

            // Relationships
            builder.HasOne(c => c.Category)
                .WithMany()
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Language)
                .WithMany()
                .HasForeignKey(c => c.LanguageId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Instructor)
                .WithMany()
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
