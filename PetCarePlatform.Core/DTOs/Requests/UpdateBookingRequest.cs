namespace PetCarePlatform.Core.DTOs.Requests
{
    public class UpdateBookingRequest
    {
        public int BookingId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? SpecialInstructions { get; set; }
    }
}
