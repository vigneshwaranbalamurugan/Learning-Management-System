using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class InstructorStakeholderConfiguration : IEntityTypeConfiguration<InstructorStakeholder>
    {
        public void Configure(EntityTypeBuilder<InstructorStakeholder> builder)
        {
            builder.ToTable("InstructorStakeholders");
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.RazorpayStakeholderId).IsUnique();
            builder.Property(x => x.RazorpayStakeholderId).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Email).HasMaxLength(255).IsRequired();
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp without time zone");
            builder.Property(x => x.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp without time zone");
        }
    }
}
