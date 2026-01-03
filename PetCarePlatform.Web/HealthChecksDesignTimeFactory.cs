using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using HealthChecks.UI.Data;

namespace PetCarePlatform.Web
{
    // Design-time factory for HealthChecksDb placed in the Web project where HealthChecks UI package is referenced
    public class HealthChecksDesignTimeFactory : IDesignTimeDbContextFactory<HealthChecksDb>
    {
        public HealthChecksDb CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                var basePath = Directory.GetCurrentDirectory();
                var appSettings = Path.Combine(basePath, "appsettings.json");

                var configBuilder = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddEnvironmentVariables();

                if (File.Exists(appSettings))
                {
                    configBuilder.AddJsonFile(appSettings, optional: true, reloadOnChange: false);
                }

                var config = configBuilder.Build();
                connectionString = config.GetConnectionString("DefaultConnection");
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Could not find a database connection string for HealthChecksDb. Set DEFAULT_CONNECTION or add DefaultConnection in appsettings.json.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<HealthChecksDb>();
            // Create migrations in the Web project (HealthChecks UI is a runtime/UI feature)
            optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("PetCarePlatform.Web"));

            return new HealthChecksDb(optionsBuilder.Options);
        }
    }
}
