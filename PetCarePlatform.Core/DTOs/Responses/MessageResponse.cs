namespace PetCarePlatform.Core.DTOs.Responses
{
    public class MessageResponse
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string? SenderPhotoUrl { get; set; }
        public int ReceiverId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string? ReceiverPhotoUrl { get; set; }
        public int? BookingId { get; set; }
        public string? BookingTitle { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public List<string> AttachmentUrls { get; set; } = new();
    }
}

