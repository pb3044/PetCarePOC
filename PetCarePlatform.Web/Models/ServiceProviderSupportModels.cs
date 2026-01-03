using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PetCarePlatform.Web.Models
{
    public class ServiceProviderInfo
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string BusinessName { get; set; }
        public string BusinessType { get; set; }
        public string Description { get; set; }
        public double? AverageRating { get; set; }
        public int? TotalReviews { get; set; }
        public string CurrentAvailabilityStatus { get; set; } = string.Empty;
        public bool IsAvailableToday { get; set; }
        public string UserName { get; set; } = string.Empty;
        // Add more fields as needed
    }

    public class ServicePhotoItem
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Caption { get; set; }
        public bool IsPrimary { get; set; }
        // Add more fields as needed
    }

    public class ServiceProviderOnboardingViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Province is required")]
        public string Province { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal code is required")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        [Display(Name = "Bio")]
        public string Bio { get; set; } = string.Empty;

        [Required(ErrorMessage = "Business name is required")]
        [Display(Name = "Business Name")]
        public string BusinessName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Business type is required")]
        [Display(Name = "Business Type")]
        public string BusinessType { get; set; } = string.Empty;

        [Display(Name = "Business Number")]
        public string BusinessNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [Display(Name = "Service Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Credentials & Qualifications")]
        public string Credentials { get; set; } = string.Empty;

        [Display(Name = "Certifications")]
        public string Certifications { get; set; } = string.Empty;

        [Display(Name = "Insurance Information")]
        public string InsuranceInfo { get; set; } = string.Empty;

        [Display(Name = "License Information")]
        public string LicenseInfo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Service area is required")]
        [Display(Name = "Service Area")]
        public string ServiceArea { get; set; } = string.Empty;

        [Required(ErrorMessage = "Service radius is required")]
        [Range(1, 100, ErrorMessage = "Service radius must be between 1 and 100 km")]
        [Display(Name = "Service Radius (km)")]
        public int ServiceRadius { get; set; } = 10;

        [Display(Name = "Banking Information")]
        public string BankingInfo { get; set; } = string.Empty;

        [Display(Name = "Tax Information")]
        public string TaxInfo { get; set; } = string.Empty;
    }

    public class ServiceProviderSettingsViewModel
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

        // Business Settings
        [Required(ErrorMessage = "Business name is required")]
        [Display(Name = "Business Name")]
        public string BusinessName { get; set; } = string.Empty;

        [Display(Name = "Business Type")]
        public string BusinessType { get; set; } = string.Empty;

        [Display(Name = "Business Number")]
        public string BusinessNumber { get; set; } = string.Empty;

        [Display(Name = "Business Description")]
        public string Description { get; set; } = string.Empty;

        // Location Settings
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Display(Name = "Province")]
        public string Province { get; set; } = string.Empty;

        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        [Display(Name = "Service Area")]
        public string ServiceArea { get; set; } = string.Empty;

        [Range(1, 100, ErrorMessage = "Service radius must be between 1 and 100 km")]
        [Display(Name = "Service Radius (km)")]
        public int ServiceRadius { get; set; } = 10;

        // Professional Settings
        [Display(Name = "Credentials & Qualifications")]
        public string Credentials { get; set; } = string.Empty;

        [Display(Name = "Certifications")]
        public string Certifications { get; set; } = string.Empty;

        [Display(Name = "Insurance Information")]
        public string InsuranceInfo { get; set; } = string.Empty;

        [Display(Name = "License Information")]
        public string LicenseInfo { get; set; } = string.Empty;

        // Financial Settings
        [Display(Name = "Banking Information")]
        public string BankingInfo { get; set; } = string.Empty;

        [Display(Name = "Tax Information")]
        public string TaxInfo { get; set; } = string.Empty;

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

        // Privacy Settings
        [Display(Name = "Profile Visibility")]
        public string ProfileVisibility { get; set; } = "Public";

        [Display(Name = "Show Contact Information")]
        public bool ShowContactInfo { get; set; } = true;

        [Display(Name = "Show Location")]
        public bool ShowLocation { get; set; } = true;

        [Display(Name = "Show Reviews")]
        public bool ShowReviews { get; set; } = true;

        // Availability Settings
        [Display(Name = "Auto-Accept Bookings")]
        public bool AutoAcceptBookings { get; set; } = false;

        [Display(Name = "Require Approval")]
        public bool RequireApproval { get; set; } = true;

        [Range(1, 365, ErrorMessage = "Max advance booking days must be between 1 and 365")]
        [Display(Name = "Max Advance Booking Days")]
        public int MaxAdvanceBookingDays { get; set; } = 30;

        [Range(1, 168, ErrorMessage = "Min advance booking hours must be between 1 and 168")]
        [Display(Name = "Min Advance Booking Hours")]
        public int MinAdvanceBookingHours { get; set; } = 2;
    }
} 