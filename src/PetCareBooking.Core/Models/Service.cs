using System;
using System.Collections.Generic;

namespace PetCareBooking.Core.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public string Category { get; set; } // Grooming, Walking, Boarding, etc.
        public int DurationMinutes { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser Provider { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
}
