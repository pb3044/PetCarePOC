using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PetCarePlatform.Core.Common;
using PetCarePlatform.Core.DTOs.Queries;
using PetCarePlatform.Core.DTOs.Requests;
using PetCarePlatform.Core.DTOs.Responses;
using PetCarePlatform.Core.Exceptions;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IServiceProviderRepository _serviceProviderRepository;
        private readonly ILogger<ReviewService> _logger;
        
        public ReviewService(
            IReviewRepository reviewRepository,
            IBookingRepository bookingRepository,
            IServiceProviderRepository serviceProviderRepository,
            ILogger<ReviewService> logger)
        {
            _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _serviceProviderRepository = serviceProviderRepository ?? throw new ArgumentNullException(nameof(serviceProviderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ============================================
        // Enterprise Pattern Methods (Result-based)
        // ============================================

        public async Task<Result<ReviewResponse>> GetReviewByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting review by ID: {ReviewId}", id);
                
                var review = await _reviewRepository.GetByIdAsync(id).ConfigureAwait(false);
                if (review == null)
                {
                    _logger.LogWarning("Review not found: {ReviewId}", id);
                    return Result<ReviewResponse>.Failure("Review not found", "REVIEW_NOT_FOUND");
                }

                var response = MapToReviewResponse(review);
                return Result<ReviewResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review {ReviewId}", id);
                return Result<ReviewResponse>.Failure("An error occurred while retrieving the review", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<ReviewResponse>> GetReviewByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting review by booking ID: {BookingId}", bookingId);
                
                var review = await _reviewRepository.GetByBookingIdAsync(bookingId).ConfigureAwait(false);
                if (review == null)
                {
                    _logger.LogWarning("Review not found for booking: {BookingId}", bookingId);
                    return Result<ReviewResponse>.Failure("Review not found for this booking", "REVIEW_NOT_FOUND");
                }

                var response = MapToReviewResponse(review);
                return Result<ReviewResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review for booking {BookingId}", bookingId);
                return Result<ReviewResponse>.Failure("An error occurred while retrieving the review", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<PagedResult<ReviewResponse>>> GetReviewsAsync(ReviewQuery query, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting reviews with query: ServiceId={ServiceId}, ProviderId={ProviderId}, Page={Page}", 
                    query.ServiceId, query.ProviderId, query.PageNumber);

                IEnumerable<Review> reviews;

                if (query.ServiceId.HasValue)
                {
                    reviews = await _reviewRepository.GetByServiceIdAsync(query.ServiceId.Value).ConfigureAwait(false);
                }
                else if (query.ProviderId.HasValue)
                {
                    reviews = await _reviewRepository.GetByRevieweeIdAsync(query.ProviderId.Value).ConfigureAwait(false);
                }
                else if (query.ReviewerId.HasValue)
                {
                    reviews = await _reviewRepository.GetByReviewerIdAsync(query.ReviewerId.Value).ConfigureAwait(false);
                }
                else
                {
                    reviews = await _reviewRepository.GetAllAsync().ConfigureAwait(false);
                }

                // Apply filters
                if (query.MinRating.HasValue)
                {
                    reviews = reviews.Where(r => r.Rating >= query.MinRating.Value);
                }

                if (query.MaxRating.HasValue)
                {
                    reviews = reviews.Where(r => r.Rating <= query.MaxRating.Value);
                }

                if (query.HasResponse.HasValue)
                {
                    reviews = reviews.Where(r => query.HasResponse.Value 
                        ? !string.IsNullOrEmpty(r.Response) 
                        : string.IsNullOrEmpty(r.Response));
                }

                if (query.FromDate.HasValue)
                {
                    reviews = reviews.Where(r => r.CreatedAt >= query.FromDate.Value);
                }

                if (query.ToDate.HasValue)
                {
                    reviews = reviews.Where(r => r.CreatedAt <= query.ToDate.Value);
                }

                // Apply sorting
                reviews = query.SortBy?.ToLower() switch
                {
                    "rating" => query.SortOrder == "asc" 
                        ? reviews.OrderBy(r => r.Rating) 
                        : reviews.OrderByDescending(r => r.Rating),
                    "createdat" => query.SortOrder == "asc" 
                        ? reviews.OrderBy(r => r.CreatedAt) 
                        : reviews.OrderByDescending(r => r.CreatedAt),
                    _ => reviews.OrderByDescending(r => r.CreatedAt)
                };

                var totalCount = reviews.Count();
                var items = reviews
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(MapToReviewResponse)
                    .ToList();

                var pagedResult = new PagedResult<ReviewResponse>(
                    items,
                    totalCount,
                    query.PageNumber,
                    query.PageSize
                );

                return Result<PagedResult<ReviewResponse>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews");
                return Result<PagedResult<ReviewResponse>>.Failure("An error occurred while retrieving reviews", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<ReviewResponse>> CreateReviewAsync(CreateReviewRequest request, int reviewerId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating review for booking {BookingId} by reviewer {ReviewerId}", 
                    request.BookingId, reviewerId);

                // Validate booking exists and is completed
                var booking = await _bookingRepository.GetByIdAsync(request.BookingId).ConfigureAwait(false);
                if (booking == null)
                {
                    _logger.LogWarning("Booking not found: {BookingId}", request.BookingId);
                    return Result<ReviewResponse>.Failure("Booking not found", "BOOKING_NOT_FOUND");
                }

                if (booking.Status != BookingStatus.Completed)
                {
                    _logger.LogWarning("Cannot review booking {BookingId} - status is {Status}", 
                        request.BookingId, booking.Status);
                    return Result<ReviewResponse>.Failure(
                        "Cannot review a booking that is not completed", 
                        "BOOKING_NOT_COMPLETED");
                }

                // Check if review already exists
                var existingReview = await _reviewRepository.GetByBookingIdAsync(request.BookingId).ConfigureAwait(false);
                if (existingReview != null)
                {
                    _logger.LogWarning("Review already exists for booking {BookingId}", request.BookingId);
                    return Result<ReviewResponse>.Failure(
                        "You have already reviewed this booking", 
                        "REVIEW_ALREADY_EXISTS");
                }

                // Validate reviewer owns the booking
                if (booking.OwnerId != reviewerId)
                {
                    _logger.LogWarning("Reviewer {ReviewerId} does not own booking {BookingId}", 
                        reviewerId, request.BookingId);
                    return Result<ReviewResponse>.Failure(
                        "You can only review your own bookings", 
                        "UNAUTHORIZED");
                }

                // Create review
                var review = new Review
                {
                    BookingId = request.BookingId,
                    ServiceId = booking.ServiceId,
                    ReviewerId = reviewerId,
                    RevieweeId = booking.Service.ProviderId,
                    Rating = request.Rating,
                    Comment = request.Comment,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdReview = await _reviewRepository.CreateAsync(review).ConfigureAwait(false);

                // Handle photo uploads if provided
                if (request.PhotoUrls != null && request.PhotoUrls.Any())
                {
                    foreach (var photoUrl in request.PhotoUrls.Take(5)) // Max 5 photos
                    {
                        var photo = new ReviewPhoto
                        {
                            ReviewId = createdReview.Id,
                            Url = photoUrl,
                            Caption = string.Empty,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _reviewRepository.AddReviewPhotoAsync(photo).ConfigureAwait(false);
                    }
                }

                // Update provider's average rating
                await CalculateAverageRatingForProviderAsync(createdReview.RevieweeId).ConfigureAwait(false);

                _logger.LogInformation("Review created successfully: {ReviewId}", createdReview.Id);

                var response = MapToReviewResponse(createdReview);
                return Result<ReviewResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review for booking {BookingId}", request.BookingId);
                return Result<ReviewResponse>.Failure("An error occurred while creating the review", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<ReviewResponse>> UpdateReviewAsync(UpdateReviewRequest request, int reviewerId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating review {ReviewId} by reviewer {ReviewerId}", 
                    request.ReviewId, reviewerId);

                var existingReview = await _reviewRepository.GetByIdAsync(request.ReviewId).ConfigureAwait(false);
                if (existingReview == null)
                {
                    _logger.LogWarning("Review not found: {ReviewId}", request.ReviewId);
                    return Result<ReviewResponse>.Failure("Review not found", "REVIEW_NOT_FOUND");
                }

                // Validate reviewer owns the review
                if (existingReview.ReviewerId != reviewerId)
                {
                    _logger.LogWarning("Reviewer {ReviewerId} does not own review {ReviewId}", 
                        reviewerId, request.ReviewId);
                    return Result<ReviewResponse>.Failure(
                        "You can only update your own reviews", 
                        "UNAUTHORIZED");
                }

                // Update fields
                existingReview.Rating = request.Rating;
                existingReview.Comment = request.Comment;
                existingReview.UpdatedAt = DateTime.UtcNow;

                await _reviewRepository.UpdateAsync(existingReview).ConfigureAwait(false);

                // Update provider's average rating
                await CalculateAverageRatingForProviderAsync(existingReview.RevieweeId).ConfigureAwait(false);

                _logger.LogInformation("Review updated successfully: {ReviewId}", request.ReviewId);

                var response = MapToReviewResponse(existingReview);
                return Result<ReviewResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating review {ReviewId}", request.ReviewId);
                return Result<ReviewResponse>.Failure("An error occurred while updating the review", "INTERNAL_ERROR");
            }
        }

        public async Task<Result> AddResponseToReviewAsync(AddReviewResponseRequest request, int revieweeId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Adding response to review {ReviewId} by reviewee {RevieweeId}", 
                    request.ReviewId, revieweeId);

                var review = await _reviewRepository.GetByIdAsync(request.ReviewId).ConfigureAwait(false);
                if (review == null)
                {
                    _logger.LogWarning("Review not found: {ReviewId}", request.ReviewId);
                    return Result.Failure("Review not found", "REVIEW_NOT_FOUND");
                }

                // Validate reviewee owns the review
                if (review.RevieweeId != revieweeId)
                {
                    _logger.LogWarning("Reviewee {RevieweeId} does not own review {ReviewId}", 
                        revieweeId, request.ReviewId);
                    return Result.Failure(
                        "You can only respond to reviews about your services", 
                        "UNAUTHORIZED");
                }

                await _reviewRepository.AddResponseAsync(request.ReviewId, request.Response).ConfigureAwait(false);

                _logger.LogInformation("Response added to review {ReviewId}", request.ReviewId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding response to review {ReviewId}", request.ReviewId);
                return Result.Failure("An error occurred while adding the response", "INTERNAL_ERROR");
            }
        }

        // ============================================
        // Analytics Methods (Enterprise Pattern)
        // ============================================

        public async Task<Result<RatingBreakdownResponse>> GetRatingBreakdownAsync(int providerId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting rating breakdown for provider {ProviderId}", providerId);

                var reviews = await _reviewRepository.GetByRevieweeIdAsync(providerId).ConfigureAwait(false);
                if (reviews == null || !reviews.Any())
                {
                    return Result<RatingBreakdownResponse>.Success(new RatingBreakdownResponse());
                }

                var breakdown = new RatingBreakdownResponse();
                foreach (var review in reviews)
                {
                    switch (review.Rating)
                    {
                        case 5: breakdown.FiveStarCount++; break;
                        case 4: breakdown.FourStarCount++; break;
                        case 3: breakdown.ThreeStarCount++; break;
                        case 2: breakdown.TwoStarCount++; break;
                        case 1: breakdown.OneStarCount++; break;
                    }
                }

                breakdown.AverageRating = reviews.Average(r => r.Rating);

                return Result<RatingBreakdownResponse>.Success(breakdown);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rating breakdown for provider {ProviderId}", providerId);
                return Result<RatingBreakdownResponse>.Failure("An error occurred while retrieving rating breakdown", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<List<RatingTrendResponse>>> GetRatingTrendsAsync(int providerId, int days = 30, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting rating trends for provider {ProviderId} for {Days} days", providerId, days);

                var reviews = await _reviewRepository.GetByRevieweeIdAsync(providerId).ConfigureAwait(false);
                if (reviews == null || !reviews.Any())
                {
                    return Result<List<RatingTrendResponse>>.Success(new List<RatingTrendResponse>());
                }

                var cutoffDate = DateTime.UtcNow.AddDays(-days);
                var recentReviews = reviews.Where(r => r.CreatedAt >= cutoffDate).OrderBy(r => r.CreatedAt);

                var trends = new List<RatingTrendResponse>();
                var groupedReviews = recentReviews.GroupBy(r => r.CreatedAt.Date);

                foreach (var group in groupedReviews)
                {
                    var averageRating = group.Average(r => r.Rating);
                    trends.Add(new RatingTrendResponse
                    {
                        Date = group.Key,
                        AverageRating = averageRating,
                        ReviewCount = group.Count(),
                        Period = "Daily"
                    });
                }

                return Result<List<RatingTrendResponse>>.Success(trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rating trends for provider {ProviderId}", providerId);
                return Result<List<RatingTrendResponse>>.Failure("An error occurred while retrieving rating trends", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<List<ServiceRatingResponse>>> GetServiceRatingsAsync(int providerId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting service ratings for provider {ProviderId}", providerId);

                var reviews = await _reviewRepository.GetByRevieweeIdAsync(providerId).ConfigureAwait(false);
                if (reviews == null || !reviews.Any())
                {
                    return Result<List<ServiceRatingResponse>>.Success(new List<ServiceRatingResponse>());
                }

                var serviceRatings = reviews
                    .GroupBy(r => r.ServiceId)
                    .Select(g => new ServiceRatingResponse
                    {
                        ServiceId = g.Key,
                        ServiceName = g.First().Service?.Title ?? "Unknown Service",
                        AverageRating = g.Average(r => r.Rating),
                        ReviewCount = g.Count(),
                        TotalBookings = g.Count() // TODO: Calculate from actual bookings
                    })
                    .OrderByDescending(sr => sr.AverageRating)
                    .ToList();

                return Result<List<ServiceRatingResponse>>.Success(serviceRatings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service ratings for provider {ProviderId}", providerId);
                return Result<List<ServiceRatingResponse>>.Failure("An error occurred while retrieving service ratings", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<ReviewPerformanceMetricsResponse>> GetPerformanceMetricsAsync(int providerId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting performance metrics for provider {ProviderId}", providerId);

                var reviews = await _reviewRepository.GetByRevieweeIdAsync(providerId).ConfigureAwait(false);
                if (reviews == null || !reviews.Any())
                {
                    return Result<ReviewPerformanceMetricsResponse>.Success(new ReviewPerformanceMetricsResponse());
                }

                var now = DateTime.UtcNow;
                var thisMonth = now.AddDays(-30);
                var lastMonth = now.AddDays(-60);

                var reviewsThisMonth = reviews.Count(r => r.CreatedAt >= thisMonth);
                var reviewsLastMonth = reviews.Count(r => r.CreatedAt >= lastMonth && r.CreatedAt < thisMonth);
                var reviewsWithResponses = reviews.Count(r => !string.IsNullOrEmpty(r.Response));

                var responseTimes = reviews
                    .Where(r => !string.IsNullOrEmpty(r.Response))
                    .Select(r => (r.UpdatedAt - r.CreatedAt).TotalHours)
                    .ToList();

                // Calculate rating improvement (compare first half vs second half of reviews)
                var sortedReviews = reviews.OrderBy(r => r.CreatedAt).ToList();
                var halfPoint = sortedReviews.Count / 2;
                var firstHalf = sortedReviews.Take(halfPoint);
                var secondHalf = sortedReviews.Skip(halfPoint);
                var firstHalfAvg = firstHalf.Any() ? firstHalf.Average(r => r.Rating) : 0;
                var secondHalfAvg = secondHalf.Any() ? secondHalf.Average(r => r.Rating) : 0;
                var ratingImprovement = secondHalfAvg - firstHalfAvg;

                var metrics = new ReviewPerformanceMetricsResponse
                {
                    ResponseRate = reviews.Any() ? (double)reviewsWithResponses / reviews.Count() * 100 : 0,
                    AverageResponseTime = responseTimes.Any() ? responseTimes.Average() : 0,
                    TotalResponses = reviewsWithResponses,
                    PendingResponses = reviews.Count() - reviewsWithResponses,
                    RatingImprovement = ratingImprovement,
                    ReviewsThisMonth = reviewsThisMonth,
                    ReviewsLastMonth = reviewsLastMonth
                };

                return Result<ReviewPerformanceMetricsResponse>.Success(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics for provider {ProviderId}", providerId);
                return Result<ReviewPerformanceMetricsResponse>.Failure("An error occurred while retrieving performance metrics", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<List<RecentReviewResponse>>> GetRecentReviewsAsync(int providerId, int count = 10, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting recent reviews for provider {ProviderId}, count {Count}", providerId, count);

                var reviews = await _reviewRepository.GetByRevieweeIdAsync(providerId).ConfigureAwait(false);
                if (reviews == null || !reviews.Any())
                {
                    return Result<List<RecentReviewResponse>>.Success(new List<RecentReviewResponse>());
                }

                var recentReviews = reviews
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(count)
                    .Select(r => new RecentReviewResponse
                    {
                        Id = r.Id,
                        ReviewerName = $"{r.Reviewer?.FirstName} {r.Reviewer?.LastName}".Trim(),
                        ServiceName = r.Service?.Title ?? "Unknown Service",
                        Rating = r.Rating,
                        Comment = r.Comment,
                        Response = r.Response ?? string.Empty,
                        CreatedAt = r.CreatedAt,
                        ResponseDate = !string.IsNullOrEmpty(r.Response) ? r.UpdatedAt : null
                    })
                    .ToList();

                return Result<List<RecentReviewResponse>>.Success(recentReviews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent reviews for provider {ProviderId}", providerId);
                return Result<List<RecentReviewResponse>>.Failure("An error occurred while retrieving recent reviews", "INTERNAL_ERROR");
            }
        }

        // Helper method to map Review to ReviewResponse
        private ReviewResponse MapToReviewResponse(Review review)
        {
            // Load photos if not already loaded
            var photos = review.Photos?.Select(p => p.Url).ToList() ?? new List<string>();
            
            return new ReviewResponse
            {
                Id = review.Id,
                BookingId = review.BookingId,
                ReviewerId = review.ReviewerId,
                ReviewerName = $"{review.Reviewer?.FirstName} {review.Reviewer?.LastName}".Trim(),
                RevieweeId = review.RevieweeId,
                RevieweeName = $"{review.Reviewee?.FirstName} {review.Reviewee?.LastName}".Trim(),
                ServiceId = review.ServiceId,
                ServiceName = review.Service?.Title ?? "Unknown Service",
                Rating = review.Rating,
                Comment = review.Comment,
                Response = review.Response,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt,
                PhotoUrls = photos
            };
        }

        // Private helper method to calculate and update provider's average rating
        private async Task<double> CalculateAverageRatingForProviderAsync(int providerId)
        {
            var reviews = await _reviewRepository.GetByRevieweeIdAsync(providerId).ConfigureAwait(false);
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
            var provider = await _serviceProviderRepository.GetByIdAsync(providerId).ConfigureAwait(false);
            if (provider != null)
            {
                provider.AverageRating = averageRating;
                provider.TotalReviews = count;
                await _serviceProviderRepository.UpdateAsync(provider).ConfigureAwait(false);
            }

            return averageRating;
        }

        public async Task<Result<PagedResult<ReviewResponse>>> GetReviewsByServiceIdAsync(int serviceId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting reviews for service: {ServiceId}", serviceId);
                
                var query = new ReviewQuery
                {
                    ServiceId = serviceId,
                    PageNumber = 1,
                    PageSize = 100
                };
                
                return await GetReviewsAsync(query, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews for service: {ServiceId}", serviceId);
                return Result<PagedResult<ReviewResponse>>.Failure("An error occurred while retrieving reviews", "INTERNAL_ERROR");
            }
        }
    }
}
