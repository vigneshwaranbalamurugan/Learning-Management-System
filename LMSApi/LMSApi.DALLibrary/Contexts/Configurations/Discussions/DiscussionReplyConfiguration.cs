using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class DiscussionReplyConfiguration : IEntityTypeConfiguration<DiscussionReplies>
    {
        public void Configure(EntityTypeBuilder<DiscussionReplies> builder)
        {
            builder.ToTable("DiscussionReplies");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.ReplyText)
                .IsRequired();
            builder.Property(r => r.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(r => r.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            // Relationships
            builder.HasOne(r => r.Discussion)
                .WithMany()
                .HasForeignKey(r => r.DiscussionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(r => r.DiscussionId);
            // Soft delete
            builder.HasQueryFilter(r => !r.IsDeleted);
        }
    }
}
