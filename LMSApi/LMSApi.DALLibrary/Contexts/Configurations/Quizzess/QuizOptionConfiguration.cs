using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class QuizOptionConfiguration : IEntityTypeConfiguration<QuizOptions>
    {
        public void Configure(EntityTypeBuilder<QuizOptions> builder)
        {
            builder.ToTable("QuizOptions");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.OptionText)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(o => o.IsCorrect)
                .HasDefaultValue(false);

            // Relationships
            builder.HasOne(o => o.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
