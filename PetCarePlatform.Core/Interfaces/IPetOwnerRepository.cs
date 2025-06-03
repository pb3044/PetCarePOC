using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IPetOwnerRepository
    {
        Task<PetOwner> GetByIdAsync(int id);
        Task<PetOwner> GetByUserIdAsync(int userId);
        Task<IEnumerable<PetOwner>> GetAllAsync();
        Task<PetOwner> CreateAsync(PetOwner petOwner);
        Task UpdateAsync(PetOwner petOwner);
        Task DeleteAsync(int id);
        Task<IEnumerable<ServiceProvider>> GetFavoriteProvidersAsync(int petOwnerId);
        Task AddFavoriteProviderAsync(int petOwnerId, int providerId);
        Task RemoveFavoriteProviderAsync(int petOwnerId, int providerId);
    }
}
