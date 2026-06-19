using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payments>
    {
        public void Configure(EntityTypeBuilder<Payments> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.ProviderOrderId)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.ProviderPaymentId)
                .HasMaxLength(200);

            builder.Property(p => p.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.Currency)
                .HasMaxLength(10)
                .HasDefaultValue("INR");

            builder.Property(p => p.Status)
                .HasDefaultValue(PaymentStatus.Pending);

            builder.Property(p => p.PaidAt)
                .HasColumnType("timestamp without time zone");

            // ── Platform Fee Snapshot Columns ──────────────────────────────────
            builder.Property(p => p.PlatformFeeAmount)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0m);

            builder.Property(p => p.InstructorAmount)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0m);

            builder.Property(p => p.FeeValueSnapshot)
                .HasColumnType("decimal(18,4)");

            builder.Property(p => p.PlatformFeeConfigId)
                .IsRequired(false);

            // ── Relationships ──────────────────────────────────────────────────
            builder.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Course)
                .WithMany()
                .HasForeignKey(p => p.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Enrollment)
                .WithMany()
                .HasForeignKey(p => p.EnrollmentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(p => p.PlatformFeeConfig)
                .WithMany()
                .HasForeignKey(p => p.PlatformFeeConfigId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(p => p.ProviderOrderId).IsUnique();
        }
    }
}
