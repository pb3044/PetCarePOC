namespace PetCarePlatform.Core.DTOs.Requests
{
    public class CreatePetOwnerRequest
    {
        public int UserId { get; set; }
        public string? PreferredServiceTypes { get; set; }
        public string? PreferredProviderAttributes { get; set; }
        public bool ReceiveMarketingEmails { get; set; } = false;
    }
}

