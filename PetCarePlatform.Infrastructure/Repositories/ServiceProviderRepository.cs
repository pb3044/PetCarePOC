using Microsoft.EntityFrameworkCore;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Infrastructure.Data;

namespace PetCarePlatform.Infrastructure.Repositories
{
    public class ServiceProviderRepository : BaseRepository<ServiceProvider>, IServiceProviderRepository
    {
        public ServiceProviderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<ServiceProvider?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(sp => sp.User)
                .Include(sp => sp.Services)
                    .ThenInclude(s => s.Photos)
                .Include(sp => sp.Services)
                    .ThenInclude(s => s.Reviews)
                .Include(sp => sp.AvailabilitySchedules)
                .Include(sp => sp.FavoritedByOwners)
                .FirstOrDefaultAsync(sp => sp.Id == id);
        }

        public async Task<ServiceProvider> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(sp => sp.User)
                .Include(sp => sp.Services)
                    .ThenInclude(s => s.Photos)
                .Include(sp => sp.Services)
                    .ThenInclude(s => s.Reviews)
                .Include(sp => sp.AvailabilitySchedules)
                .Include(sp => sp.FavoritedByOwners)
                .FirstOrDefaultAsync(sp => sp.UserId == userId);
        }

        public override async Task<IEnumerable<ServiceProvider>> GetAllAsync()
        {
            return await _dbSet
                .Include(sp => sp.User)
                .Include(sp => sp.Services)
                .OrderBy(sp => sp.BusinessName)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceProvider>> GetByServiceTypeAsync(ServiceType serviceType)
        {
            return await _dbSet
                .Where(sp => sp.Services.Any(s => s.Type == serviceType && s.IsActive))
                .Include(sp => sp.User)
                .Include(sp => sp.Services.Where(s => s.Type == serviceType && s.IsActive))
                    .ThenInclude(s => s.Photos)
                .Include(sp => sp.Services.Where(s => s.Type == serviceType && s.IsActive))
                    .ThenInclude(s => s.Reviews)
                .OrderBy(sp => sp.BusinessName)
                .ToListAsync();
        }

        public async Task<IEnumerable<ServiceProvider>> GetByLocationAsync(double latitude, double longitude, int radiusInKm)
        {
            // Location filtering would require additional properties in the ServiceProvider model
            // For now, return all providers
            return await _dbSet
                .Include(sp => sp.User)
                .Include(sp => sp.Services.Where(s => s.IsActive))
                    .ThenInclude(s => s.Photos)
                .Include(sp => sp.Services.Where(s => s.IsActive))
                    .ThenInclude(s => s.Reviews)
                .OrderBy(sp => sp.BusinessName)
                .ToListAsync();
        }

        public async Task UpdateAsync(ServiceProvider serviceProvider)
        {
            serviceProvider.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(serviceProvider);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PetOwner>> GetFavoritedByOwnersAsync(int providerId)
        {
            var provider = await _dbSet
                .Include(sp => sp.FavoritedByOwners)
                    .ThenInclude(po => po.User)
                .Include(sp => sp.FavoritedByOwners)
                    .ThenInclude(po => po.Pets)
                .FirstOrDefaultAsync(sp => sp.Id == providerId);

            return provider?.FavoritedByOwners ?? new List<PetOwner>();
        }

        public async Task UpdateAverageRatingAsync(int providerId)
        {
            var provider = await GetByIdAsync(providerId);
            if (provider != null)
            {
                var allReviews = await _context.Reviews
                    .Where(r => r.Service.ProviderId == providerId)
                    .ToListAsync();

                if (allReviews.Any())
                {
                    provider.AverageRating = allReviews.Average(r => r.Rating);
                    provider.TotalReviews = allReviews.Count;
                }
                else
                {
                    provider.AverageRating = 0;
                    provider.TotalReviews = 0;
                }

                provider.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}

