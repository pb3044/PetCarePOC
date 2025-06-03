using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IReviewService
    {
        Task<Review> GetReviewByIdAsync(int id);
        Task<Review> GetReviewByBookingIdAsync(int bookingId);
        Task<IEnumerable<Review>> GetReviewsByServiceIdAsync(int serviceId);
        Task<IEnumerable<Review>> GetReviewsByProviderIdAsync(int providerId);
        Task<Review> CreateReviewAsync(Review review);
        Task UpdateReviewAsync(Review review);
        Task AddResponseToReviewAsync(int reviewId, string response);
        Task<IEnumerable<ReviewPhoto>> GetReviewPhotosAsync(int reviewId);
        Task AddReviewPhotoAsync(ReviewPhoto photo);
        Task DeleteReviewPhotoAsync(int photoId);
        Task<double> CalculateAverageRatingForProviderAsync(int providerId);
        Task<double> CalculateAverageRatingForServiceAsync(int serviceId);
    }
}
