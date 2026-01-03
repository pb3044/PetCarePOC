namespace PetCarePlatform.Core.DTOs.Requests
{
    public class UpdateReviewRequest
    {
        public int ReviewId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}

