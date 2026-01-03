using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.DTOs.Queries
{
    public class BookingQuery
    {
        public int? OwnerId { get; set; }
        public int? ProviderId { get; set; }
        public int? ServiceId { get; set; }
        public BookingStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? UpcomingOnly { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "StartTime";
        public string? SortOrder { get; set; } = "asc";
    }
}
