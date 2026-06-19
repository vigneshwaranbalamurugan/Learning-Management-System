using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class PlatformFeeConfigConfiguration : IEntityTypeConfiguration<PlatformFeeConfig>
    {
        public void Configure(EntityTypeBuilder<PlatformFeeConfig> builder)
        {
            builder.ToTable("PlatformFeeConfigs");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Value)
                .HasColumnType("decimal(18,4)")
                .IsRequired();

            builder.Property(f => f.EffectiveFrom)
                .IsRequired()
                .HasColumnType("timestamp without time zone");

            builder.Property(f => f.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            // Composite index: efficient lookup for "get active config for this category at this time"
            builder.HasIndex(f => new { f.FeeCategory, f.EffectiveFrom });

            builder.HasOne(f => f.CreatedByAdmin)
                .WithMany()
                .HasForeignKey(f => f.CreatedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
