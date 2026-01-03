using Microsoft.EntityFrameworkCore;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Infrastructure.Data;

namespace PetCarePlatform.Infrastructure.Repositories
{
    public class BookingRepository : BaseRepository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<Booking?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(b => b.Service)
                    .ThenInclude(s => s.Provider)
                .Include(b => b.Owner)
                    .ThenInclude(o => o.User)
                .Include(b => b.Pet)
                .Include(b => b.Payment)
                .Include(b => b.Review)
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public override async Task<IEnumerable<Booking>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(b => b.Service)
                    .ThenInclude(s => s.Provider)
                .Include(b => b.Owner)
                .Include(b => b.Pet)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        // Explicit interface implementations (without CancellationToken)
        async Task<Booking> IBookingRepository.GetByIdAsync(int id)
        {
            return await GetByIdAsync(id) ?? throw new InvalidOperationException($"Booking with ID {id} not found");
        }

        async Task<IEnumerable<Booking>> IBookingRepository.GetAllAsync()
        {
            return await GetAllAsync();
        }

        async Task<Booking> IBookingRepository.CreateAsync(Booking booking)
        {
            return await CreateAsync(booking);
        }

        async Task IBookingRepository.DeleteAsync(int id)
        {
            await DeleteAsync(id);
        }

        public async Task<IEnumerable<Booking>> GetByOwnerIdAsync(int ownerId)
        {
            return await _dbSet
                .Where(b => b.OwnerId == ownerId)
                .Include(b => b.Service)
                    .ThenInclude(s => s.Provider)
                .Include(b => b.Pet)
                .Include(b => b.Payment)
                .Include(b => b.Review)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetByProviderIdAsync(int providerId)
        {
            return await _dbSet
                .Where(b => b.Service.ProviderId == providerId)
                .Include(b => b.Service)
                    .ThenInclude(s => s.Provider)
                .Include(b => b.Owner)
                    .ThenInclude(o => o.User)
                .Include(b => b.Pet)
                .Include(b => b.Payment)
                .Include(b => b.Review)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetByServiceIdAsync(int serviceId)
        {
            return await _dbSet
                .Where(b => b.ServiceId == serviceId)
                .Include(b => b.Service)
                    .ThenInclude(s => s.Provider)
                .Include(b => b.Owner)
                .Include(b => b.Pet)
                .Include(b => b.Payment)
                .Include(b => b.Review)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetByStatusAsync(BookingStatus status)
        {
            return await _dbSet
                .Where(b => b.Status == status)
                .Include(b => b.Service)
                    .ThenInclude(s => s.Provider)
                .Include(b => b.Owner)
                .Include(b => b.Pet)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetUpcomingBookingsAsync(int userId)
        {
            var currentDate = DateTime.UtcNow;
            return await _dbSet
                .Where(b => (b.OwnerId == userId || b.Service.ProviderId == userId) 
                           && b.StartTime > currentDate
                           && b.Status != BookingStatus.Cancelled)
                .Include(b => b.Service)
                    .ThenInclude(s => s.Provider)
                .Include(b => b.Owner)
                .Include(b => b.Pet)
                .OrderBy(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<bool> IsTimeSlotAvailableAsync(int serviceId, DateTime startTime, DateTime endTime)
        {
            return !await _dbSet
                .AnyAsync(b => b.ServiceId == serviceId
                              && b.Status != BookingStatus.Cancelled
                              && ((b.StartTime <= startTime && b.EndTime > startTime)
                                  || (b.StartTime < endTime && b.EndTime >= endTime)
                                  || (b.StartTime >= startTime && b.EndTime <= endTime)));
        }

        public async Task<bool> IsTimeSlotAvailableAsync(int serviceId, DateTime startTime, DateTime endTime, int excludeBookingId)
        {
            return !await _dbSet
                .AnyAsync(b => b.ServiceId == serviceId
                              && b.Id != excludeBookingId
                              && b.Status != BookingStatus.Cancelled
                              && ((b.StartTime <= startTime && b.EndTime > startTime)
                                  || (b.StartTime < endTime && b.EndTime >= endTime)
                                  || (b.StartTime >= startTime && b.EndTime <= endTime)));
        }

        public async Task UpdateStatusAsync(int id, BookingStatus status)
        {
            var booking = await GetByIdAsync(id);
            if (booking != null)
            {
                booking.Status = status;
                booking.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Booking booking)
        {
            booking.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(booking);
            await _context.SaveChangesAsync();
        }
    }
}

