using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IServiceService
    {
        Task<Service> GetServiceByIdAsync(int id);
        Task<IEnumerable<Service>> GetAllServicesAsync();
        Task<IEnumerable<Service>> SearchServicesAsync(
            string keyword = null,
            ServiceType? type = null,
            double? latitude = null,
            double? longitude = null,
            int? radiusInKm = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string petTypes = null,
            string petSizes = null);
        Task<Service> CreateServiceAsync(Service service);
        Task UpdateServiceAsync(Service service);
        Task DeleteServiceAsync(int id);
        Task<IEnumerable<ServicePhoto>> GetServicePhotosAsync(int serviceId);
        Task AddServicePhotoAsync(ServicePhoto photo);
        Task DeleteServicePhotoAsync(int photoId);
        Task SetPrimaryPhotoAsync(int serviceId, int photoId);
        Task<double> GetServiceRatingAsync(int serviceId);
        Task<IEnumerable<Review>> GetServiceReviewsAsync(int serviceId);
    }
}
