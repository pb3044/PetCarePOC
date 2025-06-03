using System;
using System.Collections.Generic;

namespace PetCarePlatform.Core.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public NotificationType Type { get; set; }
        public string ActionUrl { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        
        // Navigation properties
        public virtual User User { get; set; }
    }

    public enum NotificationType
    {
        BookingRequest,
        BookingConfirmation,
        BookingCancellation,
        PaymentConfirmation,
        NewMessage,
        NewReview,
        SystemAlert,
        Promotion
    }
}
