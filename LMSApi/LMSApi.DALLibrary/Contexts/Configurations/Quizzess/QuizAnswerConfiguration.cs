using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class QuizAnswerConfiguration : IEntityTypeConfiguration<QuizAnswers>
    {
        public void Configure(EntityTypeBuilder<QuizAnswers> builder)
        {
            builder.ToTable("QuizAnswers");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.IsCorrect)
                .HasDefaultValue(false);

            // Relationships
            builder.HasOne(a => a.Attempt)
                .WithMany(att => att.Answers)
                .HasForeignKey(a => a.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.SelectedOption)
                .WithMany()
                .HasForeignKey(a => a.SelectedOptionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
