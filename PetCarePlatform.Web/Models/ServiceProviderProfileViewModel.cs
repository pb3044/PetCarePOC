using System;
using System.Collections.Generic;

namespace PetCarePlatform.Web.Models
{
    public class ServiceProviderProfileViewModel
    {
        // Personal Info
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string Bio { get; set; }
        public string ProfilePicture { get; set; }

        // Business Info
        public string BusinessName { get; set; }
        public string BusinessType { get; set; }
        public string BusinessNumber { get; set; }
        public string Description { get; set; }
        public string Credentials { get; set; }
        public string Certifications { get; set; }
        public string InsuranceInfo { get; set; }
        public string LicenseInfo { get; set; }
        public bool BackgroundCheckVerified { get; set; }
        public DateTime? BackgroundCheckDate { get; set; }
        public bool IdentityVerified { get; set; }
        public string ServiceArea { get; set; }
        public int ServiceRadius { get; set; }
        public string BankingInfo { get; set; }
        public string TaxInfo { get; set; }
        public double? AverageRating { get; set; }
        public int? TotalReviews { get; set; }
        public string TaxNumber { get; set; }
        public string BusinessDescription { get; set; }
        public string SpecialNotes { get; set; }
        public List<ReviewViewModel> Reviews { get; set; } = new List<ReviewViewModel>();
    }
} 