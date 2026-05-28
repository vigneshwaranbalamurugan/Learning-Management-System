using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LMSApi.DALLibrary.Contexts
{
    public class LMSDbContextFactory : IDesignTimeDbContextFactory<LMSDbContext>
    {
        public LMSDbContext CreateDbContext(string[] args)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var apiProjectPath = Path.Combine(currentDirectory, "LMSApi.API");

            if (!File.Exists(Path.Combine(apiProjectPath, "appsettings.json")))
            {
                var parentDirectory = Directory.GetParent(currentDirectory)?.FullName;
                if (!string.IsNullOrWhiteSpace(parentDirectory))
                {
                    apiProjectPath = Path.Combine(parentDirectory, "LMSApi.API");
                }
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(apiProjectPath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Host=localhost;Port=5432;Database=LMSApiDesignTime;Username=postgres;Password=postgres";

            var optionsBuilder = new DbContextOptionsBuilder<LMSDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new LMSDbContext(optionsBuilder.Options);
        }
    }
}
