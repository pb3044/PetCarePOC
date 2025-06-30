using System;
using System.Collections.Generic;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Web.Models
{
    public class ServiceProviderBookingRequestViewModel
    {
        public List<BookingRequestItem> PendingRequests { get; set; } = new();
        public List<BookingRequestItem> RecentRequests { get; set; } = new();
        public BookingRequestFilters Filters { get; set; } = new();
        public int TotalPendingCount { get; set; }
        public int TotalRecentCount { get; set; }
    }

    public class BookingRequestItem
    {
        public int Id { get; set; }
        public string PetOwnerName { get; set; } = string.Empty;
        public string PetOwnerEmail { get; set; } = string.Empty;
        public string PetOwnerPhone { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string PetName { get; set; } = string.Empty;
        public string PetType { get; set; } = string.Empty;
        public string PetBreed { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Duration { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string SpecialInstructions { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string AvailabilityMessage { get; set; } = string.Empty;
        public string PetOwnerAddress { get; set; } = string.Empty;
        public double Distance { get; set; }
        public string EstimatedTravelTime { get; set; } = string.Empty;
    }

    public class BookingRequestFilters
    {
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? ServiceType { get; set; }
        public string? PetType { get; set; }
        public bool? ShowOnlyAvailable { get; set; }
        public string? SearchTerm { get; set; }
    }
} 