using Microsoft.EntityFrameworkCore;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Infrastructure.Data;

namespace PetCarePlatform.Infrastructure.Repositories
{
    public class ServiceRepository : BaseRepository<Service>, IServiceRepository
    {
        public ServiceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<Service?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(s => s.Provider)
                .Include(s => s.Photos)
                .Include(s => s.Reviews)
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public override async Task<IEnumerable<Service>> GetAllAsync()
        {
            return await _dbSet
                .Include(s => s.Provider)
                .Include(s => s.Photos)
                .Include(s => s.Reviews)
                .Where(s => s.IsActive)
                .OrderBy(s => s.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Service>> GetByProviderIdAsync(int providerId)
        {
            return await _dbSet
                .Where(s => s.ProviderId == providerId)
                .Include(s => s.Photos)
                .Include(s => s.Reviews)
                .Include(s => s.Bookings)
                .OrderBy(s => s.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Service>> GetByTypeAsync(ServiceType type)
        {
            return await _dbSet
                .Where(s => s.Type == type && s.IsActive)
                .Include(s => s.Provider)
                .Include(s => s.Photos)
                .Include(s => s.Reviews)
                .OrderBy(s => s.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Service>> SearchAsync(
            string keyword = null,
            ServiceType? type = null,
            double? latitude = null,
            double? longitude = null,
            int? radiusInKm = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string petTypes = null,
            string petSizes = null)
        {
            var query = _dbSet
                .Include(s => s.Provider)
                .Include(s => s.Photos)
                .Include(s => s.Reviews)
                .Where(s => s.IsActive);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(s => s.Title.Contains(keyword) || 
                                        s.Description.Contains(keyword) ||
                                        s.Provider.BusinessName.Contains(keyword));
            }

            if (type.HasValue)
            {
                query = query.Where(s => s.Type == type.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(s => s.BasePrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(s => s.BasePrice <= maxPrice.Value);
            }

            if (!string.IsNullOrEmpty(petTypes))
            {
                var types = petTypes.Split(',').Select(t => t.Trim()).ToList();
                query = query.Where(s => types.Any(t => s.AcceptedPetTypes.Contains(t)));
            }

            if (!string.IsNullOrEmpty(petSizes))
            {
                var sizes = petSizes.Split(',').Select(s => s.Trim()).ToList();
                query = query.Where(s => sizes.Any(size => s.AcceptedPetSizes.Contains(size)));
            }

            // For location-based search, we would need to implement distance calculation
            // This is a simplified version - in production, you might use spatial data types
            if (latitude.HasValue && longitude.HasValue && radiusInKm.HasValue)
            {
                // Simple bounding box calculation (not precise but functional for demo)
                var latRange = radiusInKm.Value / 111.0; // Rough conversion
                var lonRange = radiusInKm.Value / (111.0 * Math.Cos(latitude.Value * Math.PI / 180));
                
                query = query.Where(s => 
                    s.Latitude.HasValue && s.Longitude.HasValue &&
                    s.Latitude >= latitude.Value - latRange &&
                    s.Latitude <= latitude.Value + latRange &&
                    s.Longitude >= longitude.Value - lonRange &&
                    s.Longitude <= longitude.Value + lonRange);
            }

            return await query
                .OrderBy(s => s.Title)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingAsync(int serviceId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ServiceId == serviceId)
                .ToListAsync();

            if (!reviews.Any())
                return 0;

            return reviews.Average(r => r.Rating);
        }

        public async Task UpdateAsync(Service service)
        {
            service.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(service);
            await _context.SaveChangesAsync();
        }
    }
}

