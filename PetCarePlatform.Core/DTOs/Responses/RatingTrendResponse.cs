namespace PetCarePlatform.Core.DTOs.Responses
{
    public class RatingTrendResponse
    {
        public DateTime Date { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public string Period { get; set; } = string.Empty; // "Daily", "Weekly", "Monthly"
    }
}

