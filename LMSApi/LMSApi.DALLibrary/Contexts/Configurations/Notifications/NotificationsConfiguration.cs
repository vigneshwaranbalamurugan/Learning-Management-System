using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class NotificationsConfiguration : IEntityTypeConfiguration<Notifications>
    {
        public void Configure(EntityTypeBuilder<Notifications> builder)
        {
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Title).IsRequired().HasMaxLength(255);
            builder.Property(n => n.Message).IsRequired().HasMaxLength(1000);
            builder.Property(n => n.Type).IsRequired().HasConversion<string>();
            builder.Property(n => n.RedirectUrl).HasMaxLength(500);
            builder.Property(n => n.IsRead).HasDefaultValue(false);
            builder.Property(n => n.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
