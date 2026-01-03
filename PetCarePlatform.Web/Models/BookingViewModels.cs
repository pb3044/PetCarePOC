using System.ComponentModel.DataAnnotations;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Core.DTOs.Responses;

namespace PetCarePlatform.Web.Models
{
    public class CreateBookingViewModel
    {
        [Required]
        public int ServiceId { get; set; }
        
        public string ServiceName { get; set; }

        public int? PetId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; }

        public string Notes { get; set; }
        public string SpecialInstructions { get; set; }

        // For dropdown population
        public IEnumerable<Service> AvailableServices { get; set; } = new List<Service>();
    }

    public class BookingDetailsViewModel
    {
        public Booking Booking { get; set; }
        public bool CanCancel { get; set; }
        public bool CanReview { get; set; }
    }

    public class BookServiceViewModel
    {
        // Only the essential fields for booking creation are required
        [Required]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Please select a pet")]
        public int PetId { get; set; }

        [Required(ErrorMessage = "Please select a date")]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "Please select a start time")]
        public string StartTime { get; set; }

        [Required(ErrorMessage = "Please select an end time")]
        public string EndTime { get; set; }

        // Display-only fields - NOT required, NOT posted from form
        public string ServiceTitle { get; set; } = string.Empty;
        public string ServiceDescription { get; set; } = string.Empty;
        public decimal ServicePrice { get; set; }
        public string ServicePriceUnit { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public string ServiceLocation { get; set; } = string.Empty;

        // Optional field
        public string? SpecialInstructions { get; set; }

        // For dropdown population
        public IEnumerable<Pet> UserPets { get; set; } = new List<Pet>();
        public IEnumerable<ServiceResponse> AvailableServices { get; set; } = new List<ServiceResponse>();
    }

    public class EditBookingViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Please select a date")]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "Please select a start time")]
        public string StartTime { get; set; }

        [Required(ErrorMessage = "Please select an end time")]
        public string EndTime { get; set; }

        public string? SpecialInstructions { get; set; }

        // Display-only fields (no validation attributes)
        public string ServiceTitle { get; set; }
        public string ProviderName { get; set; }
        public string PetName { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
    }
}
