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
            }
        }
    }
}