using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AiSupport.Infrastructure.DataBase
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            var basePath = Directory.GetCurrentDirectory();
            var apiPath = Path.GetFullPath(Path.Combine(basePath, "..", "AiSupport.Api"));

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true);

            if (Directory.Exists(apiPath))
            {
                configBuilder
                    .AddJsonFile(Path.Combine(apiPath, "appsettings.json"), optional: true)
                    .AddJsonFile(Path.Combine(apiPath, "appsettings.Development.json"), optional: true);
            }

            var config = configBuilder
                .AddEnvironmentVariables()
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not found.");

            builder.UseNpgsql(connectionString, b => b.MigrationsAssembly("AiSupport.Infrastructure"));

            return new ApplicationDbContext(builder.Options);
        }
    }
}
