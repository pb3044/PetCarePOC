using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IServiceProviderService
    {
        Task<ServiceProvider> GetServiceProviderByIdAsync(int id);
        Task<ServiceProvider> GetServiceProviderByUserIdAsync(int userId);
        Task<ServiceProvider> CreateServiceProviderProfileAsync(ServiceProvider serviceProvider);
        Task UpdateServiceProviderProfileAsync(ServiceProvider serviceProvider);
        Task<IEnumerable<Service>> GetServicesByProviderIdAsync(int providerId);
        Task<Service> AddServiceAsync(Service service);
        Task UpdateServiceAsync(Service service);
        Task DeleteServiceAsync(int serviceId);
        Task<IEnumerable<Booking>> GetProviderBookingsAsync(int providerId, bool includeHistory = false);
        Task<bool> UpdateAvailabilityScheduleAsync(int providerId, IEnumerable<AvailabilitySchedule> schedules);
        Task<IEnumerable<AvailabilitySchedule>> GetAvailabilityScheduleAsync(int providerId);
        Task<IEnumerable<Review>> GetProviderReviewsAsync(int providerId);
        Task<decimal> GetProviderEarningsAsync(int providerId, DateTime? startDate = null, DateTime? endDate = null);
    }
}
