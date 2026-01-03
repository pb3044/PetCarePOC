using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PetCarePlatform.Core.Common;
using PetCarePlatform.Core.DTOs.Queries;
using PetCarePlatform.Core.DTOs.Requests;
using PetCarePlatform.Core.DTOs.Responses;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IReviewService
    {
        Task<Result<RatingBreakdownResponse>> GetRatingBreakdownAsync(int providerId, CancellationToken cancellationToken = default);
        Task<Result<List<RatingTrendResponse>>> GetRatingTrendsAsync(int providerId, int days = 30, CancellationToken cancellationToken = default);
        Task<Result<List<ServiceRatingResponse>>> GetServiceRatingsAsync(int providerId, CancellationToken cancellationToken = default);
        Task<Result<ReviewPerformanceMetricsResponse>> GetPerformanceMetricsAsync(int providerId, CancellationToken cancellationToken = default);
        Task<Result<List<RecentReviewResponse>>> GetRecentReviewsAsync(int providerId, int count = 10, CancellationToken cancellationToken = default);

        // New enterprise pattern methods
        Task<Result<ReviewResponse>> GetReviewByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<ReviewResponse>> GetReviewByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default);
        Task<Result<PagedResult<ReviewResponse>>> GetReviewsAsync(ReviewQuery query, CancellationToken cancellationToken = default);
        Task<Result<ReviewResponse>> CreateReviewAsync(CreateReviewRequest request, int reviewerId, CancellationToken cancellationToken = default);
        Task<Result<ReviewResponse>> UpdateReviewAsync(UpdateReviewRequest request, int reviewerId, CancellationToken cancellationToken = default);
        Task<Result> AddResponseToReviewAsync(AddReviewResponseRequest request, int revieweeId, CancellationToken cancellationToken = default);
        Task<Result<PagedResult<ReviewResponse>>> GetReviewsByServiceIdAsync(int serviceId, CancellationToken cancellationToken = default);
    }
}
