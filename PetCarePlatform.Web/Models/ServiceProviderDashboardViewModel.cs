using System;
using System.Collections.Generic;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Web.Models
{
    public class ServiceProviderDashboardViewModel
    {
        public int TotalBookings { get; set; }
        public int PendingRequests { get; set; }
        public int ActiveServices { get; set; }
        public decimal MonthlyEarnings { get; set; }
        public List<RecentBookingRequestViewModel> RecentRequests { get; set; } = new();
        public List<TodayScheduleViewModel> TodaySchedule { get; set; } = new();
        public ServiceProviderInfo ProviderInfo { get; set; } = new();
    }

    public class RecentBookingRequestViewModel
    {
        public int Id { get; set; }
        public string PetOwnerName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string AvailabilityMessage { get; set; } = string.Empty;
    }

    public class TodayScheduleViewModel
    {
        public int Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string PetOwnerName { get; set; } = string.Empty;
        public string PetName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Time { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string SpecialInstructions { get; set; } = string.Empty;
    }

    public class ServiceProviderInfo
    {
        public int Id { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public bool IsAvailableToday { get; set; }
        public string CurrentAvailabilityStatus { get; set; } = string.Empty;
    }
} 