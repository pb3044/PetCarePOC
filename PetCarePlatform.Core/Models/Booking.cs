using System;
using System.Collections.Generic;

namespace PetCarePlatform.Core.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public int OwnerId { get; set; }
        public int? PetId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public BookingStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty; // Fix: Initialize with a default value
        public string SpecialInstructions { get; set; } = string.Empty; // Fix: Initialize with a default value
        public decimal TotalPrice { get; set; }
        public int? PaymentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public virtual Service Service { get; set; }
        public virtual PetOwner Owner { get; set; }
        public virtual Pet Pet { get; set; }
        public virtual Payment Payment { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual Review Review { get; set; }
    }

    public enum BookingStatus
    {
        Requested,
        Confirmed,
        InProgress,
        Completed,
        Cancelled,
        Declined
    }
}
