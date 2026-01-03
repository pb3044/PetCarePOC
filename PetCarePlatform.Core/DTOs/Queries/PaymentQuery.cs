using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.DTOs.Queries
{
    public class PaymentQuery
    {
        public int? UserId { get; set; }
        public int? BookingId { get; set; }
        public PaymentStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortOrder { get; set; } = "desc";
    }
}

