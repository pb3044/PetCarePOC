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

        public override async Task<PetOwner?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(po => po.User)
                .Include(po => po.Pets)
                    .ThenInclude(p => p.Photos)
                .Include(po => po.Bookings)
                    .ThenInclude(b => b.Service)
                        .ThenInclude(s => s.Provider)
                .Include(po => po.FavoriteProviders)
                .FirstOrDefaultAsync(po => po.Id == id, cancellationToken);
        }

        public async Task<PetOwner> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(po => po.User)
                .Include(po => po.Pets)
                    .ThenInclude(p => p.Photos)
                .Include(po => po.Bookings)
                    .ThenInclude(b => b.Service)
                        .ThenInclude(s => s.Provider)
                .Include(po => po.FavoriteProviders)
                .FirstOrDefaultAsync(po => po.UserId == userId) 
                ?? throw new InvalidOperationException($"PetOwner with UserId {userId} not found");
        }

        public override async Task<IEnumerable<PetOwner>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(po => po.User)
                .Include(po => po.Pets)
                .OrderBy(po => po.User.LastName)
                .ThenBy(po => po.User.FirstName)
                .ToListAsync(cancellationToken);
        }

        // Explicit interface implementations (without CancellationToken)
        async Task<PetOwner> IPetOwnerRepository.GetByIdAsync(int id)
        {
            return await GetByIdAsync(id) ?? throw new InvalidOperationException($"PetOwner with ID {id} not found");
        }

        async Task<IEnumerable<PetOwner>> IPetOwnerRepository.GetAllAsync()
        {
            return await GetAllAsync();
        }

        async Task<PetOwner> IPetOwnerRepository.CreateAsync(PetOwner petOwner)
        {
            return await CreateAsync(petOwner);
        }

        async Task IPetOwnerRepository.DeleteAsync(int id)
        {
            await DeleteAsync(id);
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

