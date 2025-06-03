using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IPetRepository
    {
        Task<Pet> GetByIdAsync(int id);
        Task<IEnumerable<Pet>> GetAllAsync();
        Task<IEnumerable<Pet>> GetByOwnerIdAsync(int ownerId);
        Task<Pet> CreateAsync(Pet pet);
        Task UpdateAsync(Pet pet);
        Task DeleteAsync(int id);
        Task<IEnumerable<PetPhoto>> GetPetPhotosAsync(int petId);
        Task AddPetPhotoAsync(PetPhoto photo);
        Task DeletePetPhotoAsync(int photoId);
        Task SetPrimaryPhotoAsync(int petId, int photoId);
    }
}
