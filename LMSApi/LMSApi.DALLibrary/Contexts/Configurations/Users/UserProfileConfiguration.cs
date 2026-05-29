using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class UserProfileConfiguration: IEntityTypeConfiguration<UserProfiles>
    {
        public void Configure(EntityTypeBuilder<UserProfiles> builder)
        {
            builder.Property(up => up.FirstName).HasMaxLength(50);
            builder.Property(up => up.LastName).HasMaxLength(50);
            builder.Property(up => up.Bio).HasMaxLength(250);
            builder.Property(up=>up.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(up=>up.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.HasOne(up => up.User)
                .WithOne(u => u.UserProfile)
                .HasForeignKey<UserProfiles>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
}