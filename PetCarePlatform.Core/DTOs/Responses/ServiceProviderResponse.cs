namespace PetCarePlatform.Core.DTOs.Responses
{
    public class ServiceProviderResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string BusinessType { get; set; } = string.Empty;
        public string? BusinessNumber { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Credentials { get; set; }
        public string? Certifications { get; set; }
        public string? InsuranceInfo { get; set; }
        public string? LicenseInfo { get; set; }
        public bool BackgroundCheckVerified { get; set; }
        public bool IdentityVerified { get; set; }
        public bool IsVerified => BackgroundCheckVerified && IdentityVerified;
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public string? ServiceArea { get; set; }
        public int ServiceRadius { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ServiceCount { get; set; }
    }
}

