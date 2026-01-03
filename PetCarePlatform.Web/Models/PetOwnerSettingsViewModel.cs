using System.ComponentModel.DataAnnotations;

namespace PetCarePlatform.Web.Models
{
    public class PetOwnerSettingsViewModel
    {
        // Account Settings
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        // Location Settings
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Display(Name = "Province")]
        public string Province { get; set; } = string.Empty;

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        [Display(Name = "Bio")]
        public string Bio { get; set; } = string.Empty;

        // Notification Settings
        [Display(Name = "Email Notifications")]
        public bool EmailNotifications { get; set; } = true;

        [Display(Name = "SMS Notifications")]
        public bool SMSNotifications { get; set; } = false;

        [Display(Name = "Booking Reminders")]
        public bool BookingReminders { get; set; } = true;

        [Display(Name = "Payment Notifications")]
        public bool PaymentNotifications { get; set; } = true;

        [Display(Name = "Review Notifications")]
        public bool ReviewNotifications { get; set; } = true;

        [Display(Name = "Marketing Emails")]
        public bool MarketingEmails { get; set; } = false;

        // Privacy Settings
        [Display(Name = "Profile Visibility")]
        public string ProfileVisibility { get; set; } = "Public";

        [Display(Name = "Show Contact Information")]
        public bool ShowContactInfo { get; set; } = true;

        [Display(Name = "Show Location")]
        public bool ShowLocation { get; set; } = true;

        [Display(Name = "Show Pets")]
        public bool ShowPets { get; set; } = true;

        // Preferences
        [Display(Name = "Preferred Service Types")]
        public string PreferredServiceTypes { get; set; } = string.Empty;

        [Display(Name = "Preferred Providers")]
        public string PreferredProviders { get; set; } = string.Empty;

        [Range(1, 100, ErrorMessage = "Max travel distance must be between 1 and 100 km")]
        [Display(Name = "Max Travel Distance (km)")]
        public int MaxTravelDistance { get; set; } = 25;

        [Range(1, 168, ErrorMessage = "Preferred booking advance must be between 1 and 168 hours")]
        [Display(Name = "Preferred Booking Advance (hours)")]
        public int PreferredBookingAdvance { get; set; } = 24;
    }
}
