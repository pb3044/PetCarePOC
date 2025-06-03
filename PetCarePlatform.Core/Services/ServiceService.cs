using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IReviewRepository _reviewRepository;
        
        public ServiceService(IServiceRepository serviceRepository, IReviewRepository reviewRepository)
        {
            _serviceRepository = serviceRepository;
            _reviewRepository = reviewRepository;
        }

        public async Task<Service> GetServiceByIdAsync(int id)
        {
            return await _serviceRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Service>> GetAllServicesAsync()
        {
            return await _serviceRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Service>> SearchServicesAsync(
            string keyword = null, 
            ServiceType? type = null, 
            double? latitude = null, 
            double? longitude = null, 
            int? radiusInKm = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string petTypes = null,
            string petSizes = null)
        {
            return await _serviceRepository.SearchAsync(
                keyword, 
                type, 
                latitude, 
                longitude, 
                radiusInKm,
                minPrice,
                maxPrice,
                petTypes,
                petSizes);
        }

        public async Task<Service> CreateServiceAsync(Service service)
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

        public async Task DeleteServiceAsync(int id)
        {
            await _serviceRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<ServicePhoto>> GetServicePhotosAsync(int serviceId)
        {
            var service = await _serviceRepository.GetByIdAsync(serviceId);
            if (service == null)
            {
                throw new InvalidOperationException("Service not found");
            }

            return service.Photos;
        }

        public async Task AddServicePhotoAsync(ServicePhoto photo)
        {
            var service = await _serviceRepository.GetByIdAsync(photo.ServiceId);
            if (service == null)
            {
                throw new InvalidOperationException("Service not found");
            }

            // In a real implementation, we would add the photo to the database
            // For now, we'll just return
        }

        public async Task DeleteServicePhotoAsync(int photoId)
        {
            // In a real implementation, we would delete the photo from the database
            // For now, we'll just return
        }

        public async Task SetPrimaryPhotoAsync(int serviceId, int photoId)
        {
            var service = await _serviceRepository.GetByIdAsync(serviceId);
            if (service == null)
            {
                throw new InvalidOperationException("Service not found");
            }

            // In a real implementation, we would update the primary photo in the database
            // For now, we'll just return
        }

        public async Task<double> GetServiceRatingAsync(int serviceId)
        {
            return await _serviceRepository.GetAverageRatingAsync(serviceId);
        }

        public async Task<IEnumerable<Review>> GetServiceReviewsAsync(int serviceId)
        {
            return await _reviewRepository.GetByServiceIdAsync(serviceId);
        }
    }
}
