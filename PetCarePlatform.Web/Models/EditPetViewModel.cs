using System.ComponentModel.DataAnnotations;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Web.Models
{
    public class EditPetViewModel
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public PetType Type { get; set; }
        public string? Breed { get; set; }
        public int Age { get; set; }
        public string? Size { get; set; }
        public string? Gender { get; set; }
        public bool IsNeutered { get; set; }
        public string? MedicalInformation { get; set; }
        public string? SpecialNeeds { get; set; }
        public string? Temperament { get; set; }
        public string? FeedingInstructions { get; set; }
        public string? ExerciseNeeds { get; set; }
        public string? BehavioralNotes { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? VeterinarianName { get; set; }
        public string? VeterinarianPhone { get; set; }
    }
} 