using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace PetCarePlatform.Infrastructure.Configuration
{
    /// <summary>
    /// Extension methods for configuration validation and registration.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Validates and binds configuration sections.
        /// </summary>
        public static IServiceCollection AddValidatedConfiguration<T>(
            this IServiceCollection services,
            IConfiguration configuration,
            string sectionName) where T : class, new()
        {
            var section = configuration.GetSection(sectionName);
            services.Configure<T>(section);
            
            // Validate configuration on startup
            services.AddSingleton<IValidateOptions<T>>(provider =>
            {
                return new ConfigurationValidator<T>(sectionName);
            });

            return services;
        }

        /// <summary>
        /// Validates configuration on startup.
        /// </summary>
        public static void ValidateConfiguration(this IServiceProvider serviceProvider)
        {
            try
            {
                // Validate Email configuration
                var emailConfig = serviceProvider.GetRequiredService<IOptions<EmailConfiguration>>().Value;
                ValidateEmailConfiguration(emailConfig);

                // Google Maps configuration removed - using OpenStreetMap (free, no API key needed)
            }
            catch (InvalidOperationException)
            {
                // Re-throw configuration errors
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Configuration validation failed", ex);
            }
        }

        private static void ValidateEmailConfiguration(EmailConfiguration config)
        {
            // Only validate if email is enabled
            if (!config.IsEnabled)
            {
                return; // Skip validation if email is disabled
            }

            if (string.IsNullOrWhiteSpace(config.SmtpHost))
            {
                throw new InvalidOperationException("Email:SmtpHost is required when Email:IsEnabled is true");
            }

            if (config.SmtpPort <= 0 || config.SmtpPort > 65535)
            {
                throw new InvalidOperationException("Email:SmtpPort must be between 1 and 65535");
            }

            if (string.IsNullOrWhiteSpace(config.FromEmail))
            {
                throw new InvalidOperationException("Email:FromEmail is required when Email:IsEnabled is true");
            }

            // Username and Password are optional during validation - they may be in User Secrets
            // They will be checked at runtime in EmailService
        }


        private class ConfigurationValidator<T> : IValidateOptions<T> where T : class
        {
            private readonly string _sectionName;

            public ConfigurationValidator(string sectionName)
            {
                _sectionName = sectionName;
            }

            public ValidateOptionsResult Validate(string? name, T options)
            {
                if (options == null)
                {
                    return ValidateOptionsResult.Fail($"Configuration section '{_sectionName}' is null or invalid.");
                }

                return ValidateOptionsResult.Success;
            }
        }
    }
}
