using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class AuditLogsConfiguration : IEntityTypeConfiguration<AuditLogs>
    {
        public void Configure(EntityTypeBuilder<AuditLogs> builder)
        {
            builder.HasKey(al => al.Id);
            builder.Property(al => al.TableName).IsRequired().HasMaxLength(255);
            builder.Property(al => al.Action).IsRequired().HasConversion<string>();
            builder.Property(al => al.OldValues).IsRequired();
            builder.Property(al => al.NewValues).IsRequired();
            builder.Property(al => al.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasOne(al => al.User)
                .WithMany()
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
