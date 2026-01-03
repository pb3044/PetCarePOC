using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.DTOs.Requests
{
    public class CreateServiceRequest
    {
        public int ProviderId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ServiceType Type { get; set; }
        public decimal BasePrice { get; set; }
        public string PriceUnit { get; set; } = "per visit";
        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? AcceptedPetTypes { get; set; }
        public string? AcceptedPetSizes { get; set; }
        public int? MaxPetsPerBooking { get; set; }
    }
}
