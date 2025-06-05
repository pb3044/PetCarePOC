using System;
using System.Collections.Generic;

namespace PetCarePlatform.Core.Models
{
    public class ServiceProvider
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string BusinessName { get; set; }
        public string Description { get; set; }
        public string Credentials { get; set; }
        public string Certifications { get; set; }
        public string InsuranceInfo { get; set; }
        public string LicenseInfo { get; set; }
        public bool BackgroundCheckVerified { get; set; }
        public DateTime? BackgroundCheckDate { get; set; }
        public bool IdentityVerified { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public string ServiceArea { get; set; }
        public int ServiceRadius { get; set; }
        public string BankingInfo { get; set; }
        public string TaxInfo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Service> Services { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<AvailabilitySchedule> AvailabilitySchedules { get; set; }
        public virtual ICollection<PetOwner> FavoritedByOwners { get; set; }
    }
}
