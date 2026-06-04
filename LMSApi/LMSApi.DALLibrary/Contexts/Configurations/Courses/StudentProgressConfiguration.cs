using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class StudentProgressConfiguration : IEntityTypeConfiguration<StudentProgress>
    {
        public void Configure(EntityTypeBuilder<StudentProgress> builder)
        {
            builder.ToTable("StudentProgress");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.VideoWatchedPercentage)
                .HasColumnType("numeric(5,2)")
                .HasDefaultValue(0m);

            builder.Property(p => p.ProgressPercentage)
                .HasDefaultValue(0.0);

            builder.Property(p => p.IsCompleted)
                .HasDefaultValue(false);

            // Relationships
            builder.HasOne(p => p.Student)
                .WithMany()
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.Course)
                .WithMany()
                .HasForeignKey(p => p.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.Lesson)
                .WithMany()
                .HasForeignKey(p => p.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
