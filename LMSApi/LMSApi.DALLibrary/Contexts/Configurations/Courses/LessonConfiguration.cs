using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class LessonConfiguration : IEntityTypeConfiguration<Lessons>
    {
        public void Configure(EntityTypeBuilder<Lessons> builder)
        {
            builder.ToTable("Lessons");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Title)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(l => l.Description)
                .HasMaxLength(1000);

            builder.Property(l => l.VideoUrl)
                .HasMaxLength(1000);

            builder.Property(l => l.ExternalUrl)
                .HasMaxLength(1000);

            builder.Property(l => l.SortOrder)
                .HasDefaultValue(0);

            // Relationships
            builder.HasOne(l => l.CourseSection)
                .WithMany(s => s.Lessons)
                .HasForeignKey(l => l.CourseSectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
