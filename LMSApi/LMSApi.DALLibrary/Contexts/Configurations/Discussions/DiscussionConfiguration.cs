using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class DiscussionConfiguration : IEntityTypeConfiguration<Discussions>
    {
        public void Configure(EntityTypeBuilder<Discussions> builder)
        {
            builder.ToTable("Discussions");
            builder.HasKey(d => d.Id);
            builder.Property(d => d.Title)
                .HasMaxLength(255)
                .IsRequired();
            builder.Property(d => d.Content)
                .IsRequired();
            builder.Property(d => d.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(d => d.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasOne(d => d.Course)
                .WithMany()
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(d => d.Lesson)
                .WithMany(l => l.Discussions)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(d => d.LessonId);

            builder.HasQueryFilter(d => !d.IsDeleted);
        }
    }
}
