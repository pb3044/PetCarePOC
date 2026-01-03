namespace PetCarePlatform.Core.DTOs.Requests
{
    public class CancelBookingRequest
    {
        public int BookingId { get; set; }
        public string CancellationReason { get; set; } = string.Empty;
    }
}

