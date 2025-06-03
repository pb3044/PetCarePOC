using System;
using System.Collections.Generic;

namespace PetCarePlatform.Core.Models
{
    public class Pet
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string Name { get; set; }
        public PetType Type { get; set; }
        public string Breed { get; set; }
        public int Age { get; set; }
        public string Size { get; set; }
        public string Gender { get; set; }
        public bool IsNeutered { get; set; }
        public string MedicalInformation { get; set; }
        public string SpecialNeeds { get; set; }
        public string Temperament { get; set; }
        public string FeedingInstructions { get; set; }
        public string ExerciseNeeds { get; set; }
        public string BehavioralNotes { get; set; }
        public string EmergencyContactName { get; set; }
        public string EmergencyContactPhone { get; set; }
        public string VeterinarianName { get; set; }
        public string VeterinarianPhone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual PetOwner Owner { get; set; }
        public virtual ICollection<PetPhoto> Photos { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
    }

    public enum PetType
    {
        Dog,
        Cat,
        Bird,
        Fish,
        SmallAnimal,
        Reptile,
        Other
    }
}
