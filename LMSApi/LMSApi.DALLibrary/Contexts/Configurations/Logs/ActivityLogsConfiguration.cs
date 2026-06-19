using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class ActivityLogsConfiguration : IEntityTypeConfiguration<ActivityLogs>
    {
        public void Configure(EntityTypeBuilder<ActivityLogs> builder)
        {
            builder.HasKey(al => al.Id);
            builder.Property(al => al.ActivityType).IsRequired().HasConversion<string>();
            builder.Property(al => al.Description).IsRequired().HasMaxLength(1000);
            builder.Property(al => al.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasOne(al => al.User)
                .WithMany()
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
