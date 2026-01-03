namespace PetCarePlatform.Core.DTOs.Requests
{
    public class UpdatePetOwnerRequest
    {
        public int PetOwnerId { get; set; }
        public string? PreferredServiceTypes { get; set; }
        public string? PreferredProviderAttributes { get; set; }
        public bool? ReceiveMarketingEmails { get; set; }
        public bool? ReceiveNotifications { get; set; }
    }
}

