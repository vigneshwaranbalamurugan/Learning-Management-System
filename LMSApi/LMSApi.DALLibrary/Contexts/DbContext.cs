using LMSApi.ModelLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LMSApi.DALLibrary.Contexts
{
    public class LMSDbContext: DbContext
    {
        public DbSet<Users> Users { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }
        public DbSet<UserProfiles> UserProfiles { get; set; }

        // Course module
        public DbSet<CourseCategories> CourseCategories { get; set; }
        public DbSet<Courses> Courses { get; set; }
        public DbSet<CourseSection> CourseSections { get; set; }
        public DbSet<Lessons> Lessons { get; set; }
        public DbSet<LessonResources> LessonResources { get; set; }
        public DbSet<StudentProgress> StudentProgresses { get; set; }
        public DbSet<WishList> WishLists { get; set; }
        public DbSet<Reviews> Reviews { get; set; }

        // Hybrid Learning module
        public DbSet<CourseBatch> CourseBatches { get; set; }
        public DbSet<Enrollments> Enrollments { get; set; }
        public DbSet<Payments> Payments { get; set; }

        // Quiz module
        public DbSet<Quzzes> Quizzes { get; set; }
        public DbSet<QuizQuestions> QuizQuestions { get; set; }
        public DbSet<QuizOptions> QuizOptions { get; set; }
        public DbSet<QuizAttempts> QuizAttempts { get; set; }
        public DbSet<QuizAnswers> QuizAnswers { get; set; }

        // Assignment module
        public DbSet<Assignments> Assignments { get; set; }
        public DbSet<AssignmentSubmissions> AssignmentSubmissions { get; set; }

        public LMSDbContext(DbContextOptions<LMSDbContext> dbContextOptions):base(dbContextOptions)
        {
        }        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
          base.OnModelCreating(modelBuilder);
          modelBuilder.ApplyConfigurationsFromAssembly(typeof(LMSDbContext).Assembly);
        }

        public override int SaveChanges()
        {
            ApplyAuditTimestamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditTimestamps()
        {
            var utcNow = DateTime.UtcNow;
            foreach(EntityEntry entry in ChangeTracker.Entries())
            {
                if(entry.State == EntityState.Added)
                {
                    setAuditTimestamps(entry.Entity,utcNow,create:true);
                }
                else if(entry.State == EntityState.Modified)
                {
                    setAuditTimestamps(entry.Entity,utcNow,create:false);
                }
            }
        }

        private void setAuditTimestamps(object entity, DateTime utcNow,bool create)
        {
            switch (entity)
            {
                case Users user:
                    if (create) user.CreatedAt = utcNow;
                    user.UpdatedAt = utcNow;
                    break;
                case UserRoles role:
                    if (create) role.CreatedAt = utcNow;
                    role.UpdatedAt = utcNow;
                    break;
                case UserProfiles profile:
                    if (create) profile.CreatedAt = utcNow;
                    profile.UpdatedAt = utcNow;
                    break;
                case CourseCategories category:
                    if (create) category.CreatedAt = utcNow;
                    category.UpdatedAt = utcNow;
                    break;
                case Courses course:
                    if (create) course.CreatedAt = utcNow;
                    course.UpdatedAt = utcNow;
                    break;
                case CourseSection section:
                    if (create) section.CreatedAt = utcNow;
                    section.UpdatedAt = utcNow;
                    break;
                case CourseBatch batch:
                    if (create) batch.CreatedAt = utcNow;
                    batch.UpdatedAt = utcNow;
                    break;
                case StudentProgress progress:
                    progress.LastAccessed = utcNow;
                    break;
                case Assignments assignment:
                    if (create) assignment.CreatedAt = utcNow;
                    break;
                case WishList wishlist:
                    if (create) wishlist.AddedAt = utcNow;
                    wishlist.UpdatedAt = utcNow;
                    break;
                case Reviews review:
                    if (create) review.CreatedAt = utcNow;
                    review.UpdatedAt = utcNow;
                    break;
            }
        }
    }
}