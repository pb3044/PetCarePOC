using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IPetOwnerService
    {
        Task<PetOwner> GetPetOwnerByIdAsync(int id);
        Task<PetOwner> GetPetOwnerByUserIdAsync(int userId);
        Task<PetOwner> CreatePetOwnerProfileAsync(PetOwner petOwner);
        Task UpdatePetOwnerProfileAsync(PetOwner petOwner);
        Task<IEnumerable<Pet>> GetPetsByOwnerIdAsync(int ownerId);
        Task<Pet> AddPetAsync(Pet pet);
        Task UpdatePetAsync(Pet pet);
        Task DeletePetAsync(int petId);
        Task<IEnumerable<ServiceProvider>> GetFavoriteProvidersAsync(int petOwnerId);
        Task AddFavoriteProviderAsync(int petOwnerId, int providerId);
        Task RemoveFavoriteProviderAsync(int petOwnerId, int providerId);
        Task<IEnumerable<Booking>> GetOwnerBookingsAsync(int ownerId, bool includeHistory = false);
    }
}
