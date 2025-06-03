using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IServiceProviderRepository
    {
        Task<ServiceProvider> GetByIdAsync(int id);
        Task<ServiceProvider> GetByUserIdAsync(int userId);
        Task<IEnumerable<ServiceProvider>> GetAllAsync();
        Task<IEnumerable<ServiceProvider>> GetByServiceTypeAsync(ServiceType serviceType);
        Task<IEnumerable<ServiceProvider>> GetByLocationAsync(double latitude, double longitude, int radiusInKm);
        Task<ServiceProvider> CreateAsync(ServiceProvider serviceProvider);
        Task UpdateAsync(ServiceProvider serviceProvider);
        Task DeleteAsync(int id);
        Task<IEnumerable<PetOwner>> GetFavoritedByOwnersAsync(int providerId);
        Task UpdateAverageRatingAsync(int providerId);
    }
}
