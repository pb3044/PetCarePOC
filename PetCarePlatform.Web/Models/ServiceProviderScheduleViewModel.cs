using System;
using System.Collections.Generic;

namespace PetCarePlatform.Web.Models
{
    public class ServiceProviderScheduleViewModel
    {
        public List<ScheduleAppointment> Appointments { get; set; } = new();
        public List<AvailabilitySchedule> Availability { get; set; } = new();
        public ScheduleStatistics Statistics { get; set; } = new();
        public DateTime CurrentDate { get; set; } = DateTime.Now;
        public string CurrentView { get; set; } = "day"; // day, week, month
    }

    public class ScheduleAppointment
    {
        public int Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string PetOwnerName { get; set; } = string.Empty;
        public string PetName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public string SpecialInstructions { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string TimeSlot { get; set; } = string.Empty;
    }

    public class AvailabilitySchedule
    {
        public int Id { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public bool IsAvailable { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string DayName { get; set; } = string.Empty;
    }

    public class ScheduleStatistics
    {
        public int TodayAppointments { get; set; }
        public int WeekAppointments { get; set; }
        public int AvailableSlots { get; set; }
        public int BlockedHours { get; set; }
        public decimal TodayEarnings { get; set; }
        public decimal WeekEarnings { get; set; }
    }
} 