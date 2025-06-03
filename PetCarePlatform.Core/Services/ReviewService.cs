using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IServiceProviderRepository _serviceProviderRepository;
        
        public ReviewService(
            IReviewRepository reviewRepository,
            IBookingRepository bookingRepository,
            IServiceProviderRepository serviceProviderRepository)
        {
            _reviewRepository = reviewRepository;
            _bookingRepository = bookingRepository;
            _serviceProviderRepository = serviceProviderRepository;
        }

        public async Task<Review> GetReviewByIdAsync(int id)
        {
            return await _reviewRepository.GetByIdAsync(id);
        }

        public async Task<Review> GetReviewByBookingIdAsync(int bookingId)
        {
            return await _reviewRepository.GetByBookingIdAsync(bookingId);
        }

        public async Task<IEnumerable<Review>> GetReviewsByServiceIdAsync(int serviceId)
        {
            return await _reviewRepository.GetByServiceIdAsync(serviceId);
        }

        public async Task<IEnumerable<Review>> GetReviewsByProviderIdAsync(int providerId)
        {
            return await _reviewRepository.GetByRevieweeIdAsync(providerId);
        }

        public async Task<Review> CreateReviewAsync(Review review)
        {
            // Validate booking exists and is completed
            var booking = await _bookingRepository.GetByIdAsync(review.BookingId);
            if (booking == null)
            {
                throw new InvalidOperationException("Booking not found");
            }

            if (booking.Status != BookingStatus.Completed)
            {
                throw new InvalidOperationException("Cannot review a booking that is not completed");
            }

            // Validate rating is between 1 and 5
            if (review.Rating < 1 || review.Rating > 5)
            {
                throw new InvalidOperationException("Rating must be between 1 and 5");
            }

            // Set default values
            review.CreatedAt = DateTime.UtcNow;
            review.UpdatedAt = DateTime.UtcNow;

            var createdReview = await _reviewRepository.CreateAsync(review);

            // Update provider's average rating
            await CalculateAverageRatingForProviderAsync(review.RevieweeId);

            return createdReview;
        }

        public async Task UpdateReviewAsync(Review review)
        {
            var existingReview = await _reviewRepository.GetByIdAsync(review.Id);
            if (existingReview == null)
            {
                throw new InvalidOperationException("Review not found");
            }

            // Validate rating is between 1 and 5
            if (review.Rating < 1 || review.Rating > 5)
            {
                throw new InvalidOperationException("Rating must be between 1 and 5");
            }

            // Update fields
            existingReview.Rating = review.Rating;
            existingReview.Comment = review.Comment;
            existingReview.UpdatedAt = DateTime.UtcNow;

            await _reviewRepository.UpdateAsync(existingReview);

            // Update provider's average rating
            await CalculateAverageRatingForProviderAsync(existingReview.RevieweeId);
        }

        public async Task AddResponseToReviewAsync(int reviewId, string response)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
            {
                throw new InvalidOperationException("Review not found");
            }

            await _reviewRepository.AddResponseAsync(reviewId, response);
        }

        public async Task<IEnumerable<ReviewPhoto>> GetReviewPhotosAsync(int reviewId)
        {
            return await _reviewRepository.GetReviewPhotosAsync(reviewId);
        }

        public async Task AddReviewPhotoAsync(ReviewPhoto photo)
        {
            await _reviewRepository.AddReviewPhotoAsync(photo);
        }

        public async Task DeleteReviewPhotoAsync(int photoId)
        {
            await _reviewRepository.DeleteReviewPhotoAsync(photoId);
        }

        public async Task<double> CalculateAverageRatingForProviderAsync(int providerId)
        {
            var reviews = await _reviewRepository.GetByRevieweeIdAsync(providerId);
            if (reviews == null || !reviews.Any())
            {
                return 0;
            }

            double totalRating = 0;
            int count = 0;

            foreach (var review in reviews)
            {
                totalRating += review.Rating;
                count++;
            }

            double averageRating = totalRating / count;

            // Update provider's average rating
            var provider = await _serviceProviderRepository.GetByIdAsync(providerId);
            if (provider != null)
            {
                provider.AverageRating = averageRating;
                provider.TotalReviews = count;
                await _serviceProviderRepository.UpdateAsync(provider);
            }

            return averageRating;
        }

        public async Task<double> CalculateAverageRatingForServiceAsync(int serviceId)
        {
            var reviews = await _reviewRepository.GetByServiceIdAsync(serviceId);
            if (reviews == null || !reviews.Any())
            {
                return 0;
            }

            double totalRating = 0;
            int count = 0;

            foreach (var review in reviews)
            {
                totalRating += review.Rating;
                count++;
            }

            return totalRating / count;
        }
    }
}
