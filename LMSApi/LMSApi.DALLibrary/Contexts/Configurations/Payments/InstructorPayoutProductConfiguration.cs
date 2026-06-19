using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class InstructorPayoutProductConfiguration : IEntityTypeConfiguration<InstructorPayoutProduct>
    {
        public void Configure(EntityTypeBuilder<InstructorPayoutProduct> builder)
        {
            builder.ToTable("InstructorPayoutProducts");
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.RazorpayProductId).IsUnique();
            builder.Property(x => x.RazorpayProductId).HasMaxLength(100).IsRequired();
            builder.Property(x => x.AccountNumber).HasMaxLength(20).IsRequired();
            builder.Property(x => x.IfscCode).HasMaxLength(11).IsRequired();
            builder.Property(x => x.BeneficiaryName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.ProductStatus).HasMaxLength(50).HasDefaultValue("requested");
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp without time zone");
            builder.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp without time zone");
        }
    }
}
