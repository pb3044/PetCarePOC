using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PetCarePlatform.Infrastructure.Data
{
    // Provides a design-time DbContext for EF tools (migrations / update-database)
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Try environment variable first (useful for CI); then attempt to read web appsettings
            var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                var basePath = Directory.GetCurrentDirectory();
                // When EF runs with --project pointing to Infrastructure, current directory is Infrastructure
                // Try to load Web appsettings.json as fallback
                var webAppSettings = Path.Combine(basePath, "..", "PetCarePlatform.Web", "appsettings.json");

                var configBuilder = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddEnvironmentVariables();

                if (File.Exists(webAppSettings))
                {
                    configBuilder.AddJsonFile(webAppSettings, optional: true, reloadOnChange: false);
                }

                var config = configBuilder.Build();
                connectionString = config.GetConnectionString("DefaultConnection");
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Could not find a database connection string. Set the DEFAULT_CONNECTION environment variable or provide DefaultConnection in PetCarePlatform.Web/appsettings.json.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("PetCarePlatform.Infrastructure"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
