using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Services
{
    public class ServiceProviderService : IServiceProviderService
    {
        private readonly IServiceProviderRepository _serviceProviderRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentRepository _paymentRepository;
        
        public ServiceProviderService(
            IServiceProviderRepository serviceProviderRepository,
            IServiceRepository serviceRepository,
            IBookingRepository bookingRepository,
            IPaymentRepository paymentRepository)
        {
            _serviceProviderRepository = serviceProviderRepository;
            _serviceRepository = serviceRepository;
            _bookingRepository = bookingRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task<ServiceProvider> GetServiceProviderByIdAsync(int id)
        {
            return await _serviceProviderRepository.GetByIdAsync(id);
        }

        public async Task<ServiceProvider> GetServiceProviderByUserIdAsync(int userId)
        {
            return await _serviceProviderRepository.GetByUserIdAsync(userId);
        }

        public async Task<ServiceProvider> CreateServiceProviderProfileAsync(ServiceProvider serviceProvider)
        {
            // Set default values
            serviceProvider.CreatedAt = DateTime.UtcNow;
            serviceProvider.UpdatedAt = DateTime.UtcNow;
            serviceProvider.AverageRating = 0;
            serviceProvider.TotalReviews = 0;
            serviceProvider.BackgroundCheckVerified = false;
            serviceProvider.IdentityVerified = false;

            return await _serviceProviderRepository.CreateAsync(serviceProvider);
        }

        public async Task UpdateServiceProviderProfileAsync(ServiceProvider serviceProvider)
        {
            var existingProvider = await _serviceProviderRepository.GetByIdAsync(serviceProvider.Id);
            if (existingProvider == null)
            {
                throw new InvalidOperationException("Service provider profile not found");
            }

            // Update fields
            existingProvider.BusinessName = serviceProvider.BusinessName;
            existingProvider.Description = serviceProvider.Description;
            existingProvider.Credentials = serviceProvider.Credentials;
            existingProvider.Certifications = serviceProvider.Certifications;
            existingProvider.InsuranceInfo = serviceProvider.InsuranceInfo;
            existingProvider.LicenseInfo = serviceProvider.LicenseInfo;
            existingProvider.ServiceArea = serviceProvider.ServiceArea;
            existingProvider.ServiceRadius = serviceProvider.ServiceRadius;
            existingProvider.BankingInfo = serviceProvider.BankingInfo;
            existingProvider.TaxInfo = serviceProvider.TaxInfo;
            existingProvider.UpdatedAt = DateTime.UtcNow;

            await _serviceProviderRepository.UpdateAsync(existingProvider);
        }

        public async Task<IEnumerable<Service>> GetServicesByProviderIdAsync(int providerId)
        {
            return await _serviceRepository.GetByProviderIdAsync(providerId);
        }

        public async Task<Service> AddServiceAsync(Service service)
        {
            // Set default values
            service.CreatedAt = DateTime.UtcNow;
            service.UpdatedAt = DateTime.UtcNow;
            service.IsActive = true;

            return await _serviceRepository.CreateAsync(service);
        }

        public async Task UpdateServiceAsync(Service service)
        {
            var existingService = await _serviceRepository.GetByIdAsync(service.Id);
            if (existingService == null)
            {
                throw new InvalidOperationException("Service not found");
            }

            // Update fields
            existingService.Title = service.Title;
            existingService.Description = service.Description;
            existingService.Type = service.Type;
            existingService.BasePrice = service.BasePrice;
            existingService.PriceUnit = service.PriceUnit;
            existingService.IsActive = service.IsActive;
            existingService.Location = service.Location;
            existingService.Latitude = service.Latitude;
            existingService.Longitude = service.Longitude;
            existingService.AcceptedPetTypes = service.AcceptedPetTypes;
            existingService.AcceptedPetSizes = service.AcceptedPetSizes;
            existingService.MaxPetsPerBooking = service.MaxPetsPerBooking;
            existingService.UpdatedAt = DateTime.UtcNow;

            await _serviceRepository.UpdateAsync(existingService);
        }

        public async Task DeleteServiceAsync(int serviceId)
        {
            await _serviceRepository.DeleteAsync(serviceId);
        }

        public async Task<IEnumerable<Booking>> GetProviderBookingsAsync(int providerId, bool includeHistory = false)
        {
            // This would typically filter by status based on includeHistory parameter
            return await _bookingRepository.GetByProviderIdAsync(providerId);
        }

        public async Task<bool> UpdateAvailabilityScheduleAsync(int providerId, IEnumerable<AvailabilitySchedule> schedules)
        {
            var provider = await _serviceProviderRepository.GetByIdAsync(providerId);
            if (provider == null)
            {
                throw new InvalidOperationException("Service provider not found");
            }

            // In a real implementation, we would update the availability schedules in the database
            // For now, we'll just return true
            return true;
        }

        public async Task<IEnumerable<AvailabilitySchedule>> GetAvailabilityScheduleAsync(int providerId)
        {
            var provider = await _serviceProviderRepository.GetByIdAsync(providerId);
            if (provider == null)
            {
                throw new InvalidOperationException("Service provider not found");
            }

            return provider.AvailabilitySchedules;
        }

        public async Task<IEnumerable<Review>> GetProviderReviewsAsync(int providerId)
        {
            var provider = await _serviceProviderRepository.GetByIdAsync(providerId);
            if (provider == null)
            {
                throw new InvalidOperationException("Service provider not found");
            }

            // In a real implementation, we would query the reviews from the database
            // For now, we'll just return an empty list
            return new List<Review>();
        }

        public async Task<decimal> GetProviderEarningsAsync(int providerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            return await _paymentRepository.GetProviderEarningsAsync(providerId);
        }
    }
}
