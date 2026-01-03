using Microsoft.Extensions.DependencyInjection;
using PetCarePlatform.Infrastructure.Configuration;

namespace PetCarePlatform.Web.Extensions
{
    /// <summary>
    /// Extension methods for service collection configuration.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Validates application configuration on startup.
        /// </summary>
        public static IServiceCollection ValidateConfiguration(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            
            try
            {
                // Validate Email configuration
                var emailConfig = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<EmailConfiguration>>();
                if (string.IsNullOrWhiteSpace(emailConfig.Value.SmtpHost))
                {
                    throw new InvalidOperationException("Email:SmtpHost is required");
                }

                // Google Maps configuration removed - using OpenStreetMap (free, no API key needed)

                return services;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Configuration validation failed. Please check your appsettings.json file.", ex);
            }
        }
    }
}

