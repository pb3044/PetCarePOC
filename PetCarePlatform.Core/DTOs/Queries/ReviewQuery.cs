namespace PetCarePlatform.Core.DTOs.Queries
{
    public class ReviewQuery
    {
        public int? ServiceId { get; set; }
        public int? ProviderId { get; set; }
        public int? ReviewerId { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
        public bool? HasResponse { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortOrder { get; set; } = "desc";
    }
}

