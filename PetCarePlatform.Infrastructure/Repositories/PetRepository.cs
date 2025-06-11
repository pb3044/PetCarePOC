using Microsoft.EntityFrameworkCore;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Infrastructure.Data;

namespace PetCarePlatform.Infrastructure.Repositories
{
    public class PetRepository : BaseRepository<Pet>, IPetRepository
    {
        public PetRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<Pet?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Owner)
                .Include(p => p.Photos)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public override async Task<IEnumerable<Pet>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.Owner)
                .Include(p => p.Photos)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Pet>> GetByOwnerIdAsync(int ownerId)
        {
            return await _dbSet
                .Where(p => p.OwnerId == ownerId)
                .Include(p => p.Photos)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task UpdateAsync(Pet pet)
        {
            pet.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(pet);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PetPhoto>> GetPetPhotosAsync(int petId)
        {
            return await _context.PetPhotos
                .Where(pp => pp.PetId == petId)
                .OrderByDescending(pp => pp.IsPrimary)
                .ThenBy(pp => pp.CreatedAt)
                .ToListAsync();
        }

        public async Task AddPetPhotoAsync(PetPhoto photo)
        {
            photo.CreatedAt = DateTime.UtcNow;
            await _context.PetPhotos.AddAsync(photo);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePetPhotoAsync(int photoId)
        {
            var photo = await _context.PetPhotos.FindAsync(photoId);
            if (photo != null)
            {
                _context.PetPhotos.Remove(photo);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetPrimaryPhotoAsync(int petId, int photoId)
        {
            // First, set all photos for this pet to non-primary
            var allPhotos = await _context.PetPhotos
                .Where(pp => pp.PetId == petId)
                .ToListAsync();

            foreach (var photo in allPhotos)
            {
                photo.IsPrimary = photo.Id == photoId;
            }

            await _context.SaveChangesAsync();
        }
    }
}

