using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<Users>
    {
        public void Configure(EntityTypeBuilder<Users> builder)
        {
            builder.HasIndex(u => u.Email).IsUnique();
            builder.Property(u => u.Email).IsRequired().HasMaxLength(255);
            builder.Property(u => u.PasswordHash).IsRequired();
            builder.Property(u => u.PasswordSalt).HasMaxLength(100);
            builder.Property(u => u.IsActive).HasDefaultValue(true);
            builder.Property(u => u.IsEmailVerified).HasDefaultValue(false);
            builder.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(u => u.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.HasOne(u => u.Role)
                .WithMany(ur => ur.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasData(
                new Users{
                    Id = 1,
                    Email = "admin@gmail.com",
                    PasswordHash = "TPZPlYPS43ldK8EYFX67pHzyMNFmt69wd9N2cUNObYs=",
                    PasswordSalt="K5CamRmhDuuJEyr50OpNsA==",
                    IsActive = true,
                    IsEmailVerified = true,
                    RoleId = 3,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
            });
        }
    }
}