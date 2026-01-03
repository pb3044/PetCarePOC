namespace PetCarePlatform.Core.DTOs.Requests
{
    public class SendMessageRequest
    {
        public int ReceiverId { get; set; }
        public int? BookingId { get; set; }
        public string Content { get; set; } = string.Empty;
        public List<string>? AttachmentUrls { get; set; }
    }
}

