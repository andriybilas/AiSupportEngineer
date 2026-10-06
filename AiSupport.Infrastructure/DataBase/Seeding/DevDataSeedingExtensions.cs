using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AiSupport.Infrastructure.DataBase.Seeding
{
    public static class DevDataSeedingExtensions
    {
        /// <summary>
        /// Seeds development demo data. Call only in the Development environment.
        /// Disable with configuration "Seeding:Enabled" = false (env var Seeding__Enabled=false).
        /// Failures are logged and do not stop the application from starting.
        /// </summary>
        public static async Task SeedDevelopmentDataAsync(
            this IHost host,
            CancellationToken cancellationToken = default)
        {
            var logger = host.Services
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("AiSupport.Infrastructure.DevDataSeeding");

            var configuration = host.Services.GetRequiredService<IConfiguration>();
            var enabled = configuration.GetValue("Seeding:Enabled", true);

            if (!enabled)
            {
                logger.LogInformation("Development data seeding is disabled (Seeding:Enabled = false).");
                return;
            }

            await using var scope = host.Services.CreateAsyncScope();

            var seeder = ActivatorUtilities.CreateInstance<DevDataSeeder>(scope.ServiceProvider);

            try
            {
                await seeder.SeedAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Development data seeding failed.");
            }
        }
    }
}
