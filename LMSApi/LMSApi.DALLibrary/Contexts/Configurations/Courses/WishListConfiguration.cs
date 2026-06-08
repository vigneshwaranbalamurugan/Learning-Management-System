
using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class WishListConfiguration : IEntityTypeConfiguration<WishList>
    {
        public void Configure(EntityTypeBuilder<WishList> builder)
        {
            builder.ToTable("WishLists");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.AddedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(w => w.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(w => w.User)
                .WithMany() // Assuming Users don't have a direct WishList collection to avoid cyclic dependencies unless specified
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(w => w.Course)
                .WithMany() // Assuming Courses don't have a direct WishList collection
                .HasForeignKey(w => w.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // A user can only add a specific course to their wishlist once
            builder.HasIndex(w => new { w.UserId, w.CourseId })
                .IsUnique();
        }
    }
}
