using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LMSApi.ModelLibrary.Models;

namespace LMSApi.DALLibrary.Contexts.Configurations
{
    public class CourseLanguageConfiguration : IEntityTypeConfiguration<CourseLanguages>
    {
        public void Configure(EntityTypeBuilder<CourseLanguages> builder)
        {
            builder.ToTable("CourseLanguages");

            builder.HasKey(c => c.Id);

            builder.HasIndex(c => c.Name).IsUnique();

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(50);

            // Seed initial data
            builder.HasData(
                new CourseLanguages { Id = 1, Name = "English" },
                new CourseLanguages { Id = 2, Name = "Tamil" },
                new CourseLanguages { Id = 3, Name = "Hindi" }
            );
        }
    }
}
