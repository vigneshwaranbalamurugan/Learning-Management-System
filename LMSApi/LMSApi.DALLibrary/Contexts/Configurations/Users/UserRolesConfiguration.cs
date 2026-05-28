using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class UserRolesConfiguration : IEntityTypeConfiguration<UserRoles>
    {
        public void Configure(EntityTypeBuilder<UserRoles> builder)
        {
            builder.Property(ur => ur.RoleName).IsRequired().HasMaxLength(50);
            builder.Property(ur => ur.Description).HasMaxLength(150);
            builder.Property(ur => ur.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(ur => ur.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.HasMany(ur => ur.Users)
                .WithOne(u => u.Role)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasData(
              new UserRoles
              {
                  Id = 1,
                  RoleName = "Learner",
                  Description = "Learner account",
                  CreatedAt = DateTime.UtcNow,
                  UpdatedAt = DateTime.UtcNow
              },
              new UserRoles
              {
                  Id = 2,
                  RoleName = "Instructor",
                  Description = "Instructor account",
                  CreatedAt = DateTime.UtcNow,
                  UpdatedAt = DateTime.UtcNow
              },
              new UserRoles
              {
                  Id = 3,
                  RoleName = "Admin",
                  Description = "Admin account",
                  CreatedAt = DateTime.UtcNow,
                  UpdatedAt = DateTime.UtcNow
              }
            );
        }
    }
}