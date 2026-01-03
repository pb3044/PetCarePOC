namespace PetCarePlatform.Core.DTOs.Responses
{
    public class RecentReviewResponse
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

