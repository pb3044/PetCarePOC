using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.DTOs.Responses
{
    public class BookingResponse
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public int? PetId { get; set; }
        public string? PetName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public string? SpecialInstructions { get; set; }
        public string? Notes { get; set; }
        public int? PaymentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool CanCancel { get; set; }
        public bool CanReview { get; set; }
    }
}
