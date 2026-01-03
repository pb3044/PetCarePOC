using System;
using System.Collections.Generic;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Web.Models
{
    public class RatingAnalyticsViewModel
    {
        public int ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public RatingBreakdownViewModel RatingBreakdown { get; set; } = new();
        public List<RatingTrendViewModel> RatingTrends { get; set; } = new();
        public List<ServiceRatingViewModel> ServiceRatings { get; set; } = new();
        public ReviewPerformanceMetrics PerformanceMetrics { get; set; } = new();
        public List<RecentReviewViewModel> RecentReviews { get; set; } = new();
    }

    public class RatingBreakdownViewModel
    {
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
        
        // Calculated percentages
        public double FiveStarPercentage => TotalReviews > 0 ? (double)FiveStarCount / TotalReviews * 100 : 0;
        public double FourStarPercentage => TotalReviews > 0 ? (double)FourStarCount / TotalReviews * 100 : 0;
        public double ThreeStarPercentage => TotalReviews > 0 ? (double)ThreeStarCount / TotalReviews * 100 : 0;
        public double TwoStarPercentage => TotalReviews > 0 ? (double)TwoStarCount / TotalReviews * 100 : 0;
        public double OneStarPercentage => TotalReviews > 0 ? (double)OneStarCount / TotalReviews * 100 : 0;
        
        public int TotalReviews => FiveStarCount + FourStarCount + ThreeStarCount + TwoStarCount + OneStarCount;

        public static RatingBreakdownViewModel FromDto(RatingBreakdownDto dto)
        {
            return new RatingBreakdownViewModel
            {
                FiveStarCount = dto.FiveStarCount,
                FourStarCount = dto.FourStarCount,
                ThreeStarCount = dto.ThreeStarCount,
                TwoStarCount = dto.TwoStarCount,
                OneStarCount = dto.OneStarCount
            };
        }
    }

    public class RatingTrendViewModel
    {
        public DateTime Date { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public string Period { get; set; } = string.Empty; // "Daily", "Weekly", "Monthly"

        public static RatingTrendViewModel FromDto(RatingTrendDto dto)
        {
            return new RatingTrendViewModel
            {
                Date = dto.Date,
                AverageRating = dto.AverageRating,
                ReviewCount = dto.ReviewCount,
                Period = dto.Period
            };
        }
    }

    public class ServiceRatingViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int TotalBookings { get; set; }
        public double ConversionRate => TotalBookings > 0 ? (double)ReviewCount / TotalBookings * 100 : 0;

        public static ServiceRatingViewModel FromDto(ServiceRatingDto dto)
        {
            return new ServiceRatingViewModel
            {
                ServiceId = dto.ServiceId,
                ServiceName = dto.ServiceName,
                AverageRating = dto.AverageRating,
                ReviewCount = dto.ReviewCount,
                TotalBookings = dto.TotalBookings
            };
        }
    }

    public class ReviewPerformanceMetrics
    {
        public double ResponseRate { get; set; } // Percentage of reviews responded to
        public double AverageResponseTime { get; set; } // Hours to respond
        public int TotalResponses { get; set; }
        public int PendingResponses { get; set; }
        public double RatingImprovement { get; set; } // Rating change over time
        public int ReviewsThisMonth { get; set; }
        public int ReviewsLastMonth { get; set; }
        public double MonthOverMonthGrowth => ReviewsLastMonth > 0 ? (double)(ReviewsThisMonth - ReviewsLastMonth) / ReviewsLastMonth * 100 : 0;

        public static ReviewPerformanceMetrics FromDto(ReviewPerformanceMetricsDto dto)
        {
            return new ReviewPerformanceMetrics
            {
                ResponseRate = dto.ResponseRate,
                AverageResponseTime = dto.AverageResponseTime,
                TotalResponses = dto.TotalResponses,
                PendingResponses = dto.PendingResponses,
                RatingImprovement = dto.RatingImprovement,
                ReviewsThisMonth = dto.ReviewsThisMonth,
                ReviewsLastMonth = dto.ReviewsLastMonth
            };
        }
    }

    public class RecentReviewViewModel
    {
        public int Id { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ResponseDate { get; set; }
        public bool HasResponse => !string.IsNullOrEmpty(Response);
        public List<ReviewPhotoViewModel> Photos { get; set; } = new();

        public static RecentReviewViewModel FromDto(RecentReviewDto dto)
        {
            return new RecentReviewViewModel
            {
                Id = dto.Id,
                ReviewerName = dto.ReviewerName,
                ServiceName = dto.ServiceName,
                Rating = dto.Rating,
                Comment = dto.Comment,
                Response = dto.Response,
                CreatedAt = dto.CreatedAt,
                ResponseDate = dto.ResponseDate
            };
        }
    }

    public class AnalyticsDashboardViewModel
    {
        public RatingAnalyticsViewModel RatingAnalytics { get; set; } = new();
        public List<ChartDataViewModel> RatingChartData { get; set; } = new();
        public List<ChartDataViewModel> TrendChartData { get; set; } = new();
        public List<ChartDataViewModel> ServicePerformanceData { get; set; } = new();
    }

    public class ChartDataViewModel
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Color { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
    }

    public class ReviewInsightsViewModel
    {
        public List<string> CommonPositiveKeywords { get; set; } = new();
        public List<string> CommonNegativeKeywords { get; set; } = new();
        public List<string> ImprovementSuggestions { get; set; } = new();
        public double SentimentScore { get; set; } // -1 to 1 scale
        public string OverallSentiment => SentimentScore switch
        {
            > 0.3 => "Very Positive",
            > 0.1 => "Positive",
            > -0.1 => "Neutral",
            > -0.3 => "Negative",
            _ => "Very Negative"
        };
    }
}
