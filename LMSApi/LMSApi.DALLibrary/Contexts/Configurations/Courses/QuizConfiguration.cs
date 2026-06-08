using LMSApi.ModelLibrary.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class QuizConfiguration : IEntityTypeConfiguration<Quzzes>
    {
        public void Configure(EntityTypeBuilder<Quzzes> builder)
        {
            builder.ToTable("Quizzes");

            builder.HasKey(q => q.Id);

            builder.Property(q => q.Title)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(q => q.Description)
                .HasMaxLength(2000);

            builder.Property(q => q.Order)
                .HasDefaultValue(0);

            builder.Property(q => q.IsPublished)
                .HasDefaultValue(false);

            builder.Property(q => q.MaxAttempts)
                .HasDefaultValue(1);

            builder.Property(q => q.DeadlineInDays)
                .HasDefaultValue(0);

            // Relationships
            builder.HasOne(q => q.CourseSection)
                .WithMany(s => s.Quizzes)
                .HasForeignKey(q => q.CourseSectionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(q => q.Questions)
                .WithOne(qq => qq.Quiz)
                .HasForeignKey(qq => qq.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(q => q.Attempts)
                .WithOne(a => a.Quiz)
                .HasForeignKey(a => a.QuizId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
