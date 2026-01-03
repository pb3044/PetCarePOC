namespace PetCarePlatform.Core.DTOs.Responses
{
    public class ServiceRatingResponse
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int TotalBookings { get; set; }
        public double ConversionRate => TotalBookings > 0 ? (double)ReviewCount / TotalBookings * 100 : 0;
    }
}

