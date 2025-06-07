using System.ComponentModel.DataAnnotations;
using PetCarePlatform.Core.Models;

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
}

