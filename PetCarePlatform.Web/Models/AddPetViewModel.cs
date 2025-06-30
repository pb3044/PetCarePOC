using System.ComponentModel.DataAnnotations;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Web.Models
{
    public class AddPetViewModel
    {
        [Required(ErrorMessage = "Pet name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pet type is required")]
        public PetType Type { get; set; }

        [StringLength(100, ErrorMessage = "Breed cannot be longer than 100 characters")]
        public string? Breed { get; set; }

        [Range(0, 30, ErrorMessage = "Age must be between 0 and 30")]
        public int Age { get; set; }

        [StringLength(50, ErrorMessage = "Size cannot be longer than 50 characters")]
        public string? Size { get; set; }

        [StringLength(20, ErrorMessage = "Gender cannot be longer than 20 characters")]
        public string? Gender { get; set; }

        public bool IsNeutered { get; set; }

        [StringLength(500, ErrorMessage = "Medical information cannot be longer than 500 characters")]
        public string? MedicalInformation { get; set; }

        [StringLength(500, ErrorMessage = "Special needs cannot be longer than 500 characters")]
        public string? SpecialNeeds { get; set; }

        [StringLength(200, ErrorMessage = "Temperament cannot be longer than 200 characters")]
        public string? Temperament { get; set; }

        [StringLength(500, ErrorMessage = "Feeding instructions cannot be longer than 500 characters")]
        public string? FeedingInstructions { get; set; }

        [StringLength(500, ErrorMessage = "Exercise needs cannot be longer than 500 characters")]
        public string? ExerciseNeeds { get; set; }

        [StringLength(500, ErrorMessage = "Behavioral notes cannot be longer than 500 characters")]
        public string? BehavioralNotes { get; set; }

        [StringLength(100, ErrorMessage = "Emergency contact name cannot be longer than 100 characters")]
        public string? EmergencyContactName { get; set; }

        [StringLength(20, ErrorMessage = "Emergency contact phone cannot be longer than 20 characters")]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(100, ErrorMessage = "Veterinarian name cannot be longer than 100 characters")]
        public string? VeterinarianName { get; set; }

        [StringLength(20, ErrorMessage = "Veterinarian phone cannot be longer than 20 characters")]
        public string? VeterinarianPhone { get; set; }
    }
} 