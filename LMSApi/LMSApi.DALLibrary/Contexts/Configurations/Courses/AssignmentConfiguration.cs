using LMSApi.ModelLibrary.Models;
using LMSApi.ModelLibrary.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class AssignmentConfiguration : IEntityTypeConfiguration<Assignments>
    {
        public void Configure(EntityTypeBuilder<Assignments> builder)
        {
            builder.ToTable("Assignments");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(a => a.Description)
                .HasMaxLength(2000)
                .IsRequired(false);

            builder.Property(a => a.Instructions)
                .HasMaxLength(5000)
                .IsRequired(false);

            builder.Property(a => a.AttachmentUrl)
                .IsRequired(false);

            builder.Property(a => a.DeadlineInDays)
                .HasDefaultValue(0);

            builder.Property(a => a.MaxSubmissions)
                .HasDefaultValue(1);

            builder.Property(a => a.IsLateSubmissionAllowed)
                .HasDefaultValue(false);

            builder.Property(a => a.IsCompulsory)
                .HasDefaultValue(false);

            builder.Property(a => a.Status)
                .HasDefaultValue(PublishStatus.Draft);

            builder.Property(a => a.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(a => a.CourseSection)
                .WithMany(s => s.Assignments)
                .HasForeignKey(a => a.CourseSectionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(a => a.Submissions)
                .WithOne(s => s.Assignment)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
