using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class InstructorPayoutAccountConfiguration : IEntityTypeConfiguration<InstructorPayoutAccount>
    {
        public void Configure(EntityTypeBuilder<InstructorPayoutAccount> builder)
        {
            builder.ToTable("InstructorPayoutAccounts");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.RazorpayLinkedAccountId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.LegalBusinessName).HasMaxLength(150);
            builder.Property(a => a.ContactName).HasMaxLength(100);
            builder.Property(a => a.Email).HasMaxLength(150);
            builder.Property(a => a.Phone).HasMaxLength(20);
            builder.Property(a => a.AccountNumber).HasMaxLength(50);
            builder.Property(a => a.IfscCode).HasMaxLength(20);

            builder.Property(a => a.IsActive)
                .HasDefaultValue(true);

            builder.Property(a => a.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            builder.Property(a => a.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            builder.HasIndex(a => new { a.InstructorId, a.IsActive });

            builder.HasOne(a => a.Instructor)
                .WithMany()
                .HasForeignKey(a => a.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
