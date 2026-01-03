namespace PetCarePlatform.Core.DTOs.Requests
{
    public class CreateServiceProviderRequest
    {
        public int UserId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string BusinessType { get; set; } = string.Empty;
        public string? BusinessNumber { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Credentials { get; set; }
        public string? Certifications { get; set; }
        public string? InsuranceInfo { get; set; }
        public string? LicenseInfo { get; set; }
        public string? ServiceArea { get; set; }
        public int ServiceRadius { get; set; } = 10;
    }
}

