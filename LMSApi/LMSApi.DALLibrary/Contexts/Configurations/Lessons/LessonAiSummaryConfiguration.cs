using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class LessonAiSummaryConfiguration : IEntityTypeConfiguration<LessonAiSummary>
    {
        public void Configure(EntityTypeBuilder<LessonAiSummary> builder)
        {
            builder.ToTable("LessonAiSummaries");

            builder.HasKey(s => s.Id);

            // Each lesson has at most one summary
            builder.HasIndex(s => s.LessonId).IsUnique();

            builder.Property(s => s.Summary)
                .HasColumnType("text")
                .HasDefaultValue(string.Empty);

            builder.Property(s => s.KeyPointsJson)
                .HasColumnType("text")
                .HasDefaultValue("[]");

            builder.Property(s => s.Notes)
                .HasColumnType("text")
                .HasDefaultValue(string.Empty);

            builder.Property(s => s.Status)
                .HasMaxLength(50)
                .HasDefaultValue("generating");

            builder.Property(s => s.GeneratedAt)
                .HasDefaultValueSql("NOW()");

            // Relationship: cascade delete when lesson is deleted
            builder.HasOne(s => s.Lesson)
                .WithOne()
                .HasForeignKey<LessonAiSummary>(s => s.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
