using System.ComponentModel.DataAnnotations;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Web.Models
{
    public class SearchServicesViewModel
    {
        public string Keyword { get; set; }
        public ServiceType? Type { get; set; }
        public string Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? RadiusInKm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        
        // New filtering options
        public DateTime? AvailableDate { get; set; }
        public string AvailableTime { get; set; }
        public double? MinRating { get; set; }
        public string PetType { get; set; }
        public string PetSize { get; set; }
        public string SortBy { get; set; } = "distance"; // distance, rating, price_low, price_high, newest
        public bool ShowOnlyAvailable { get; set; } = true;
        public bool ShowVerifiedOnly { get; set; } = false;

        public IEnumerable<Service> Results { get; set; } = new List<Service>();
        
        // Quick filter options
        public List<string> QuickFilters { get; set; } = new List<string>();
        public string SelectedQuickFilter { get; set; }
    }

    public class CreateServiceViewModel
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public ServiceType ServiceType { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal BasePrice { get; set; }

        [Required]
        public string PriceUnit { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Max pets per booking must be greater than 0")]
        public int MaxPetsPerBooking { get; set; } = 1;

        public string AcceptedPetTypes { get; set; }
        public string AcceptedPetSizes { get; set; }
        public string Location { get; set; }
    }

    public class EditServiceViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public ServiceType ServiceType { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal BasePrice { get; set; }

        [Required]
        public string PriceUnit { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Max pets per booking must be greater than 0")]
        public int MaxPetsPerBooking { get; set; }

        public string AcceptedPetTypes { get; set; }
        public string AcceptedPetSizes { get; set; }
        public string Location { get; set; }
        public bool IsActive { get; set; }
    }
}

