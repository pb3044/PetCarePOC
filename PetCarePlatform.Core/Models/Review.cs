using System;
using System.Collections.Generic;

namespace PetCarePlatform.Core.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int ReviewerId { get; set; }
        public int RevieweeId { get; set; }
        public int ServiceId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public string Response { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual Booking Booking { get; set; }
        public virtual User Reviewer { get; set; }
        public virtual User Reviewee { get; set; }
        public virtual Service Service { get; set; }
        public virtual ICollection<ReviewPhoto> Photos { get; set; }
    }
}
