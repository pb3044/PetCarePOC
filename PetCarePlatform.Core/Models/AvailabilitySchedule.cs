using System;
using System.Collections.Generic;

namespace PetCarePlatform.Core.Models
{
    public class AvailabilitySchedule
    {
        public int Id { get; set; }
        public int ProviderId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ServiceProvider Provider { get; set; }
    }
}
