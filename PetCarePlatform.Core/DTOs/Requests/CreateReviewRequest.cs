namespace PetCarePlatform.Core.DTOs.Requests
{
    public class CreateReviewRequest
    {
        public int BookingId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public List<string>? PhotoUrls { get; set; }
    }
}

