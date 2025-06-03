using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IServiceRepository
    {
        Task<Service> GetByIdAsync(int id);
        Task<IEnumerable<Service>> GetAllAsync();
        Task<IEnumerable<Service>> GetByProviderIdAsync(int providerId);
        Task<IEnumerable<Service>> GetByTypeAsync(ServiceType type);
        Task<IEnumerable<Service>> SearchAsync(
            string keyword = null, 
            ServiceType? type = null, 
            double? latitude = null, 
            double? longitude = null, 
            int? radiusInKm = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string petTypes = null,
            string petSizes = null);
        Task<Service> CreateAsync(Service service);
        Task UpdateAsync(Service service);
        Task DeleteAsync(int id);
        Task<double> GetAverageRatingAsync(int serviceId);
    }
}
