using System;
using System.Collections.Generic;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Web.Models
{
    public class ServiceProviderMyServicesViewModel
    {
        public List<ServiceItem> Services { get; set; } = new();
        public ServiceFormModel NewService { get; set; } = new();
        public ServiceFormModel EditService { get; set; } = new();
        public int TotalServices { get; set; }
        public int ActiveServices { get; set; }
        public int InactiveServices { get; set; }
    }

    public class ServiceItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ServiceType Type { get; set; }
        public decimal BasePrice { get; set; }
        public string PriceUnit { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int TotalBookings { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public string AcceptedPetTypes { get; set; } = string.Empty;
        public string AcceptedPetSizes { get; set; } = string.Empty;
        public int MaxPetsPerBooking { get; set; }
        public List<ServicePhotoItem> Photos { get; set; } = new();
        public string PrimaryPhotoUrl { get; set; } = string.Empty;
    }

    public class ServicePhotoItem
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }

    public class ServiceFormModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ServiceType Type { get; set; }
        public decimal BasePrice { get; set; }
        public string PriceUnit { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string AcceptedPetTypes { get; set; } = string.Empty;
        public string AcceptedPetSizes { get; set; } = string.Empty;
        public int MaxPetsPerBooking { get; set; } = 1;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
} 