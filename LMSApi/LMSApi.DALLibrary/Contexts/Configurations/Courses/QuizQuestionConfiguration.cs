using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestions>
    {
        public void Configure(EntityTypeBuilder<QuizQuestions> builder)
        {
            builder.ToTable("QuizQuestions");

            builder.HasKey(q => q.Id);

            builder.Property(q => q.QuestionText)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(q => q.Explanation)
                .HasMaxLength(2000);

            builder.Property(q => q.SortOrder)
                .HasDefaultValue(0);

            // Relationships
            builder.HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(q => q.Answers)
                .WithOne(o => o.Question)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
