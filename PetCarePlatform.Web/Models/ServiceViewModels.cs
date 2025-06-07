using System.ComponentModel.DataAnnotations;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Web.Models
{
    public class SearchServicesViewModel
    {
        public string Keyword { get; set; }
        public ServiceType? Type { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? RadiusInKm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public IEnumerable<Service> Results { get; set; } = new List<Service>();
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

