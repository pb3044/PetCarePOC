namespace PetCarePlatform.Core.DTOs.Responses
{
    public class PetOwnerResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string? PreferredServiceTypes { get; set; }
        public string? PreferredProviderAttributes { get; set; }
        public bool ReceiveMarketingEmails { get; set; }
        public bool ReceiveNotifications { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int PetCount { get; set; }
        public int BookingCount { get; set; }
    }
}

