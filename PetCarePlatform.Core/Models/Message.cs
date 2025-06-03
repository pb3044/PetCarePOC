using System;
using System.Collections.Generic;

namespace PetCarePlatform.Core.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public int? BookingId { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        
        // Navigation properties
        public virtual User Sender { get; set; }
        public virtual User Receiver { get; set; }
        public virtual Booking Booking { get; set; }
    }
}
