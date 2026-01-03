using Microsoft.Extensions.DependencyInjection;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Infrastructure.Data;
using PetCarePlatform.Infrastructure.Repositories;
using PetCarePlatform.Infrastructure.Email;
using PetCarePlatform.Infrastructure.Receipt;

namespace PetCarePlatform.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

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

            // Register email service
            services.AddScoped<IEmailService, EmailService>();

            // Register receipt service
            services.AddScoped<IReceiptService, ReceiptService>();

            return services;
        }
    }
}

