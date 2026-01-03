using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.DTOs.Responses
{
    public class ServiceResponse
    {
        public int Id { get; set; }
        public int ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string ProviderBusinessName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ServiceType Type { get; set; }
        public decimal BasePrice { get; set; }
        public string PriceUnit { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? AcceptedPetTypes { get; set; }
        public string? AcceptedPetSizes { get; set; }
        public int? MaxPetsPerBooking { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string> PhotoUrls { get; set; } = new();
    }
}
