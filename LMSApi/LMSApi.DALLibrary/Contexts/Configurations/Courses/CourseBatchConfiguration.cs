using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class CourseBatchConfiguration : IEntityTypeConfiguration<CourseBatch>
    {
        public void Configure(EntityTypeBuilder<CourseBatch> builder)
        {
            builder.ToTable("CourseBatches");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(b => b.StartDate)
                .IsRequired();

            builder.Property(b => b.EndDate)
                .IsRequired();

            builder.Property(b => b.EnrollmentStartDate)
                .IsRequired();

            builder.Property(b => b.EnrollmentEndDate)
                .IsRequired();

            builder.Property(b => b.MaxStudents)
                .IsRequired();

            builder.Property(b => b.Status)
                .HasDefaultValue(BatchStatus.Upcoming);

            builder.Property(b => b.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(b => b.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // AvailableSeats is [NotMapped] — no configuration needed.

            // Relationships
            builder.HasOne(b => b.Course)
                .WithMany(c => c.Batches)
                .HasForeignKey(b => b.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
