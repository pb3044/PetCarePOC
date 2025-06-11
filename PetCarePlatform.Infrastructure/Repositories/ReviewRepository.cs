using Microsoft.EntityFrameworkCore;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Infrastructure.Data;

namespace PetCarePlatform.Infrastructure.Repositories
{
    public class ReviewRepository : BaseRepository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<Review?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .Include(r => r.Service)
                .Include(r => r.Booking)
                .Include(r => r.Photos)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public override async Task<IEnumerable<Review>> GetAllAsync()
        {
            return await _dbSet
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .Include(r => r.Service)
                .Include(r => r.Photos)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByReviewerIdAsync(int reviewerId)
        {
            return await _dbSet
                .Where(r => r.ReviewerId == reviewerId)
                .Include(r => r.Reviewee)
                .Include(r => r.Service)
                .Include(r => r.Booking)
                .Include(r => r.Photos)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByRevieweeIdAsync(int revieweeId)
        {
            return await _dbSet
                .Where(r => r.RevieweeId == revieweeId)
                .Include(r => r.Reviewer)
                .Include(r => r.Service)
                .Include(r => r.Booking)
                .Include(r => r.Photos)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByServiceIdAsync(int serviceId)
        {
            return await _dbSet
                .Where(r => r.ServiceId == serviceId)
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .Include(r => r.Booking)
                .Include(r => r.Photos)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review> GetByBookingIdAsync(int bookingId)
        {
            return await _dbSet
                .Include(r => r.Reviewer)
                .Include(r => r.Reviewee)
                .Include(r => r.Service)
                .Include(r => r.Photos)
                .FirstOrDefaultAsync(r => r.BookingId == bookingId);
        }

        public async Task UpdateAsync(Review review)
        {
            review.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(review);
            await _context.SaveChangesAsync();
        }

        public async Task AddResponseAsync(int reviewId, string response)
        {
            var review = await GetByIdAsync(reviewId);
            if (review != null)
            {
                review.Response = response;
                review.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ReviewPhoto>> GetReviewPhotosAsync(int reviewId)
        {
            return await _context.ReviewPhotos
                .Where(rp => rp.ReviewId == reviewId)
                .OrderBy(rp => rp.CreatedAt)
                .ToListAsync();
        }

        public async Task AddReviewPhotoAsync(ReviewPhoto photo)
        {
            photo.CreatedAt = DateTime.UtcNow;
            await _context.ReviewPhotos.AddAsync(photo);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteReviewPhotoAsync(int photoId)
        {
            var photo = await _context.ReviewPhotos.FindAsync(photoId);
            if (photo != null)
            {
                _context.ReviewPhotos.Remove(photo);
                await _context.SaveChangesAsync();
            }
        }
    }
}

