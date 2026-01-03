using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.DTOs.Requests
{
    public class CreateNotificationRequest
    {
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public string? ActionUrl { get; set; }
    }
}

