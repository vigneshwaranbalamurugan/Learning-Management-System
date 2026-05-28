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
    }
}