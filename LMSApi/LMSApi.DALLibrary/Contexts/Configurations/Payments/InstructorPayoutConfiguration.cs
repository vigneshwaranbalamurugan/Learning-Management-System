using LMSApi.ModelLibrary.Enums;
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class InstructorPayoutConfiguration : IEntityTypeConfiguration<InstructorPayout>
    {
        public void Configure(EntityTypeBuilder<InstructorPayout> builder)
        {
            builder.ToTable("InstructorPayouts");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.RazorpayPayoutId)
                .HasMaxLength(100);

            builder.Property(p => p.RazorpayFundAccountId)
                .HasMaxLength(100);

            builder.Property(p => p.Status)
                .HasDefaultValue(PayoutStatus.Pending);

            builder.Property(p => p.FailureReason)
                .HasMaxLength(500);

            builder.Property(p => p.Notes)
                .HasMaxLength(1000);

            builder.Property(p => p.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            builder.Property(p => p.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            // Index for webhook lookups by Razorpay Payout ID
            builder.HasIndex(p => p.RazorpayPayoutId);

            // Index for instructor revenue queries
            builder.HasIndex(p => p.InstructorId);

            builder.HasOne(p => p.Payment)
                .WithMany(pay => pay.InstructorPayouts)
                .HasForeignKey(p => p.PaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Instructor)
                .WithMany()
                .HasForeignKey(p => p.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.PayoutAccount)
                .WithMany()
                .HasForeignKey(p => p.InstructorPayoutAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
