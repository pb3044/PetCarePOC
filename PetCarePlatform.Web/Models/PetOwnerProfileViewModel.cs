using System.ComponentModel.DataAnnotations;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Web.Models
{
    public class PetOwnerProfileViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        // Personal Information
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot be longer than 500 characters")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot be longer than 100 characters")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Province is required")]
        public string Province { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal code is required")]
        [StringLength(20, ErrorMessage = "Postal code cannot be longer than 20 characters")]
        public string PostalCode { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Bio cannot be longer than 1000 characters")]
        public string? Bio { get; set; }

        // PetOwner Preferences
        public bool ReceiveNotifications { get; set; }
        public bool ReceiveMarketingEmails { get; set; }
    }
} 