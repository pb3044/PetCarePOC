using System;
using System.Collections.Generic;

namespace PetCareBooking.Core.Models
{
    public class Pet
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } // Dog, Cat, Bird, etc.
        public string Breed { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public decimal Weight { get; set; }
        public string MedicalConditions { get; set; }
        public string SpecialNeeds { get; set; }
        public string Temperament { get; set; }
        public string VaccinationStatus { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser Owner { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
}
