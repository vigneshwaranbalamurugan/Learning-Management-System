using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollments>
    {
        public void Configure(EntityTypeBuilder<Enrollments> builder)
        {
            builder.ToTable("Enrollments");

            builder.HasKey(e => e.Id);

            // Prevent duplicate enrollments for the same user + course
            builder.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();

            builder.Property(e => e.EnrolledAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(e => e.ProgressPercentage)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0m);

            builder.Property(e => e.IsCompleted)
                .HasDefaultValue(false);

            builder.Property(e => e.EnrollmentStatus)
                .HasDefaultValue(EnrollmentStatus.Active);
            
            builder.Property(e => e.AccessExpiresAt)
                .HasColumnType("timestamp without time zone");

            // BatchId and AccessExpiresAt are nullable — no extra constraint needed.

            // Relationships
            builder.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Course)
                .WithMany()
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Batch)
                .WithMany(b => b.Enrollments)
                .HasForeignKey(e => e.BatchId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
