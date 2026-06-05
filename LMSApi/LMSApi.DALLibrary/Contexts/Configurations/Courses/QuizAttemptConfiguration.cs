using LMSApi.ModelLibrary.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempts>
    {
        public void Configure(EntityTypeBuilder<QuizAttempts> builder)
        {
            builder.ToTable("QuizAttempts");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Status)
                .HasConversion<string>()
                .HasDefaultValue(AttemptStatus.InProgress);

            builder.Property(a => a.StartedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(a => a.CompletedAt)
                .IsRequired(false);

            builder.Property(a => a.Score)
                .HasDefaultValue(0.0);

            builder.Property(a => a.IsPassed)
                .HasDefaultValue(false);

            // Relationships
            builder.HasOne(a => a.Quiz)
                .WithMany(q => q.Attempts)
                .HasForeignKey(a => a.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(a => a.Answers)
                .WithOne(ans => ans.Attempt)
                .HasForeignKey(ans => ans.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
