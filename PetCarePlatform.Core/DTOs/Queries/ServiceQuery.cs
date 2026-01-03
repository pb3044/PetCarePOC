using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.DTOs.Queries
{
    public class ServiceQuery
    {
        public int? ProviderId { get; set; }
        public ServiceType? Type { get; set; }
        public string? Keyword { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? RadiusInKm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? PetTypes { get; set; }
        public string? PetSizes { get; set; }
        public double? MinRating { get; set; }
        public bool? ShowOnlyAvailable { get; set; }
        public bool? ShowVerifiedOnly { get; set; }
        public DateTime? AvailableDate { get; set; }
        public string? AvailableTime { get; set; }
        public string? SortBy { get; set; } = "distance";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
