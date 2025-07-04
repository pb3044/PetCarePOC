using System;
using System.Collections.Generic;

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
} 