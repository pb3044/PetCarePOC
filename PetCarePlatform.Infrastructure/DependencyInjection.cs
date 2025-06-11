using Microsoft.Extensions.DependencyInjection;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Infrastructure.Repositories;

namespace PetCarePlatform.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<IPetRepository, PetRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IPetOwnerRepository, PetOwnerRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IServiceProviderRepository, ServiceProviderRepository>();

            return services;
        }
    }
}

