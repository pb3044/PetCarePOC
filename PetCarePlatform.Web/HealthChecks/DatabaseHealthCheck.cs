using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PetCarePlatform.Infrastructure.Data;

namespace PetCarePlatform.Web.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly ApplicationDbContext _context;

        public DatabaseHealthCheck(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
                
                if (canConnect)
                {
                    // Try a simple query
                    await _context.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
                    return HealthCheckResult.Healthy("Database is available");
                }
                else
                {
                    return HealthCheckResult.Unhealthy("Database is not available");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Database health check failed", ex);
            }
        }
    }
}

