using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IReviewRepository
    {
        Task<Review> GetByIdAsync(int id);
        Task<IEnumerable<Review>> GetAllAsync();
        Task<IEnumerable<Review>> GetByReviewerIdAsync(int reviewerId);
        Task<IEnumerable<Review>> GetByRevieweeIdAsync(int revieweeId);
        Task<IEnumerable<Review>> GetByServiceIdAsync(int serviceId);
        Task<Review> GetByBookingIdAsync(int bookingId);
        Task<Review> CreateAsync(Review review);
        Task UpdateAsync(Review review);
        Task DeleteAsync(int id);
        Task AddResponseAsync(int reviewId, string response);
        Task<IEnumerable<ReviewPhoto>> GetReviewPhotosAsync(int reviewId);
        Task AddReviewPhotoAsync(ReviewPhoto photo);
        Task DeleteReviewPhotoAsync(int photoId);
    }
}
