using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class InstructorLinkedAccountConfiguration : IEntityTypeConfiguration<InstructorLinkedAccount>
    {
        public void Configure(EntityTypeBuilder<InstructorLinkedAccount> builder)
        {
            builder.ToTable("InstructorLinkedAccounts");
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.InstructorId).IsUnique(false);
            builder.HasIndex(x => x.RazorpayAccountId).IsUnique();
            builder.Property(x => x.RazorpayAccountId).HasMaxLength(100).IsRequired();
            builder.Property(x => x.LegalBusinessName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.BusinessType).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ContactName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Email).HasMaxLength(255).IsRequired();
            builder.Property(x => x.Phone).HasMaxLength(15).IsRequired();
            builder.Property(x => x.Street1).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Street2).HasMaxLength(500);
            builder.Property(x => x.City).HasMaxLength(100).IsRequired();
            builder.Property(x => x.State).HasMaxLength(100).IsRequired();
            builder.Property(x => x.PostalCode).HasMaxLength(10).IsRequired();
            builder.Property(x => x.Country).HasMaxLength(2).IsRequired();
            builder.Property(x => x.Pan).HasMaxLength(10).IsRequired(false);
            builder.Property(x => x.Gst).HasMaxLength(15);
            builder.Property(x => x.AccountStatus).HasMaxLength(50).HasDefaultValue("created");
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp without time zone");
            builder.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp without time zone");
            
            builder.HasOne(x => x.Instructor)
                .WithMany()
                .HasForeignKey(x => x.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(x => x.Stakeholder)
                .WithOne(s => s.LinkedAccount)
                .HasForeignKey<InstructorStakeholder>(s => s.InstructorLinkedAccountId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasOne(x => x.PayoutProduct)
                .WithOne(p => p.LinkedAccount)
                .HasForeignKey<InstructorPayoutProduct>(p => p.InstructorLinkedAccountId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
