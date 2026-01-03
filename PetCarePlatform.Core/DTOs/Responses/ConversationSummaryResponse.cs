namespace PetCarePlatform.Core.DTOs.Responses
{
    public class ConversationSummaryResponse
    {
        public int PartnerId { get; set; }
        public string PartnerName { get; set; } = string.Empty;
        public string? PartnerPhotoUrl { get; set; }
        public string LastMessagePreview { get; set; } = string.Empty;
        public DateTime? LastMessageDate { get; set; }
        public int UnreadCount { get; set; }
        public bool IsRead { get; set; }
    }
}

