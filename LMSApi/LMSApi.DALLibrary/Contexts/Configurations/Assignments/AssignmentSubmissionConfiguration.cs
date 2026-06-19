using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class AssignmentSubmissionConfiguration : IEntityTypeConfiguration<AssignmentSubmissions>
    {
        public void Configure(EntityTypeBuilder<AssignmentSubmissions> builder)
        {
            builder.ToTable("AssignmentSubmissions");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Status)
                .HasConversion<string>()
                .HasDefaultValue(SubmissionStatus.Pending);

            builder.Property(s => s.AttemptNumber)
                .HasDefaultValue(1);

            builder.Property(s => s.IsPassed)
                .IsRequired(false);

            builder.Property(s => s.MarksAwarded)
                .IsRequired(false);

            builder.Property(s => s.Feedback)
                .IsRequired(false);

            builder.Property(s => s.SubmissionText)
                .IsRequired(false);

            builder.Property(s => s.SubmittedAssignmentUrl)
                .IsRequired(false);

            builder.Property(s => s.SubmittedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(s => s.GradedAt)
                .IsRequired(false);

            // Relationships
            builder.HasOne(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
