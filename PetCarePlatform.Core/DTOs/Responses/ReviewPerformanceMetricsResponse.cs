namespace PetCarePlatform.Core.DTOs.Responses
{
    public class ReviewPerformanceMetricsResponse
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
}

