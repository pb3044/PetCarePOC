using System;
using System.Collections.Generic;

namespace PetCarePlatform.Core.Models
{
    public class Service
    {
        public int Id { get; set; }
        public int ProviderId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public ServiceType Type { get; set; }
        public decimal BasePrice { get; set; }
        public string PriceUnit { get; set; } // Per hour, per day, per visit, etc.
        public bool IsActive { get; set; }
        public string Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string AcceptedPetTypes { get; set; } // Comma-separated list or JSON
        public string AcceptedPetSizes { get; set; } // Comma-separated list or JSON
        public int MaxPetsPerBooking { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ServiceProvider Provider { get; set; }
        public virtual ICollection<ServicePhoto> Photos { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }

    public enum ServiceType
    {
        DogWalking,
        PetSitting,
        Boarding,
        DayCare,
        Grooming,
        Training,
        PetTaxi,
        VeterinaryVisit,
        Other
    }
}
