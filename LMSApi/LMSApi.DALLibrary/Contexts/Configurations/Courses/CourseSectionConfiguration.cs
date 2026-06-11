using LMSApi.ModelLibrary.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class CourseSectionConfiguration : IEntityTypeConfiguration<CourseSection>
    {
        public void Configure(EntityTypeBuilder<CourseSection> builder)
        {
            builder.ToTable("CourseSections");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Title)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(s => s.Description)
                .HasMaxLength(1000);

            builder.Property(s => s.Status)
                .HasDefaultValue(PublishStatus.Draft);

            builder.Property(s => s.SortOrder)
                .HasDefaultValue(1);

            builder.Property(s => s.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(s => s.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(s => s.Course)
                .WithMany(c => c.Sections)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
