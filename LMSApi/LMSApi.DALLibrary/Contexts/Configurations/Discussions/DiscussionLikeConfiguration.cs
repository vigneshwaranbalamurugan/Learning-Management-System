using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class DiscussionLikeConfiguration : IEntityTypeConfiguration<DiscussionLikes>
    {
        public void Configure(EntityTypeBuilder<DiscussionLikes> builder)
        {
            builder.ToTable("DiscussionLikes");
            builder.HasKey(l => l.Id);
            builder.Property(l => l.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            // Relationships
            builder.HasOne(l => l.Discussion)
                .WithMany()
                .HasForeignKey(l => l.DiscussionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            // A user can only like a discussion once
            builder.HasIndex(l => new { l.DiscussionId, l.UserId })
                .IsUnique();
        }
    }
}
