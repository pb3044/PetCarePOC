using Microsoft.EntityFrameworkCore;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Infrastructure.Data;

namespace PetCarePlatform.Infrastructure.Repositories
{
    public class PetOwnerRepository : BaseRepository<PetOwner>, IPetOwnerRepository
    {
        public PetOwnerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<PetOwner?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(po => po.User)
                .Include(po => po.Pets)
                    .ThenInclude(p => p.Photos)
                .Include(po => po.Bookings)
                    .ThenInclude(b => b.Service)
                .Include(po => po.FavoriteProviders)
                .FirstOrDefaultAsync(po => po.Id == id);
        }

        public async Task<PetOwner> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(po => po.User)
                .Include(po => po.Pets)
                    .ThenInclude(p => p.Photos)
                .Include(po => po.Bookings)
                    .ThenInclude(b => b.Service)
                .Include(po => po.FavoriteProviders)
                .FirstOrDefaultAsync(po => po.UserId == userId);
        }

        public override async Task<IEnumerable<PetOwner>> GetAllAsync()
        {
            return await _dbSet
                .Include(po => po.User)
                .Include(po => po.Pets)
                .OrderBy(po => po.User.LastName)
                .ThenBy(po => po.User.FirstName)
                .ToListAsync();
        }

        public async Task UpdateAsync(PetOwner petOwner)
        {
            petOwner.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(petOwner);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ServiceProvider>> GetFavoriteProvidersAsync(int petOwnerId)
        {
            var petOwner = await _dbSet
                .Include(po => po.FavoriteProviders)
                    .ThenInclude(sp => sp.User)
                .Include(po => po.FavoriteProviders)
                    .ThenInclude(sp => sp.Services)
                .FirstOrDefaultAsync(po => po.Id == petOwnerId);

            return petOwner?.FavoriteProviders ?? new List<ServiceProvider>();
        }

        public async Task AddFavoriteProviderAsync(int petOwnerId, int providerId)
        {
            var petOwner = await _dbSet
                .Include(po => po.FavoriteProviders)
                .FirstOrDefaultAsync(po => po.Id == petOwnerId);

            var provider = await _context.ServiceProviders
                .FirstOrDefaultAsync(sp => sp.Id == providerId);

            if (petOwner != null && provider != null && 
                !petOwner.FavoriteProviders.Any(fp => fp.Id == providerId))
            {
                petOwner.FavoriteProviders.Add(provider);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFavoriteProviderAsync(int petOwnerId, int providerId)
        {
            var petOwner = await _dbSet
                .Include(po => po.FavoriteProviders)
                .FirstOrDefaultAsync(po => po.Id == petOwnerId);

            if (petOwner != null)
            {
                var provider = petOwner.FavoriteProviders
                    .FirstOrDefault(fp => fp.Id == providerId);

                if (provider != null)
                {
                    petOwner.FavoriteProviders.Remove(provider);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}

