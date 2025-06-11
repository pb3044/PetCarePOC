using Microsoft.EntityFrameworkCore;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Infrastructure.Data;

namespace PetCarePlatform.Infrastructure.Repositories
{
    public class PaymentRepository : BaseRepository<PetCarePlatform.Core.Models.Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<PetCarePlatform.Core.Models.Payment?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Service)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Owner)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public override async Task<IEnumerable<PetCarePlatform.Core.Models.Payment>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Service)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Owner)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<PetCarePlatform.Core.Models.Payment> GetByBookingIdAsync(int bookingId)
        {
            return await _dbSet
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Service)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Owner)
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);
        }

        public async Task<IEnumerable<PetCarePlatform.Core.Models.Payment>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(p => p.Booking.OwnerId == userId || p.Booking.Service.ProviderId == userId)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Service)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Owner)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PetCarePlatform.Core.Models.Payment>> GetByStatusAsync(PaymentStatus status)
        {
            return await _dbSet
                .Where(p => p.Status == status)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Service)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Owner)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<PetCarePlatform.Core.Models.Payment> CreateAsync(PetCarePlatform.Core.Models.Payment payment)
        {
            payment.CreatedAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;
            await _dbSet.AddAsync(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task UpdateAsync(PetCarePlatform.Core.Models.Payment payment)
        {
            payment.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(payment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int id, PaymentStatus status)
        {
            var payment = await GetByIdAsync(id);
            if (payment != null)
            {
                payment.Status = status;
                payment.UpdatedAt = DateTime.UtcNow;
                
                if (status == PaymentStatus.Captured)
                {
                    // No ProcessedAt property in the model
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _dbSet
                .Where(p => p.Status == PaymentStatus.Captured)
                .SumAsync(p => p.Amount);
        }

        public async Task<decimal> GetProviderEarningsAsync(int providerId)
        {
            return await _dbSet
                .Where(p => p.Booking.Service.ProviderId == providerId && p.Status == PaymentStatus.Captured)
                .SumAsync(p => p.Amount);
        }
    }
}

