namespace PetCarePlatform.Core.DTOs.Requests
{
    public class CreateBookingRequest
    {
        public int ServiceId { get; set; }
        public int OwnerId { get; set; }
        public int? PetId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? SpecialInstructions { get; set; }
        public decimal? TotalPrice { get; set; } // Optional, will be calculated if not provided
    }
}
