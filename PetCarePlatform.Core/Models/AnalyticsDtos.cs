using System;
using System.Collections.Generic;

namespace PetCarePlatform.Core.Models
{
    public class RatingBreakdownDto
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
    }

    public class RatingTrendDto
    {
        public DateTime Date { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public string Period { get; set; } = string.Empty; // "Daily", "Weekly", "Monthly"
    }

    public class ServiceRatingDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int TotalBookings { get; set; }
        public double ConversionRate => TotalBookings > 0 ? (double)ReviewCount / TotalBookings * 100 : 0;
    }

    public class ReviewPerformanceMetricsDto
    {
        public double ResponseRate { get; set; } // Percentage of reviews responded to
        public double AverageResponseTime { get; set; } // Hours to respond
        public int TotalResponses { get; set; }
        public int PendingResponses { get; set; }
        public double RatingImprovement { get; set; } // Rating change over time
        public int ReviewsThisMonth { get; set; }
        public int ReviewsLastMonth { get; set; }
        public double MonthOverMonthGrowth => ReviewsLastMonth > 0 ? (double)(ReviewsThisMonth - ReviewsLastMonth) / ReviewsLastMonth * 100 : 0;
    }

    public class RecentReviewDto
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
    }
}
