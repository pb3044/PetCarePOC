using System;
using System.Collections.Generic;

namespace PetCareBooking.Core.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } // Pending, Confirmed, Completed, Cancelled
        public string Notes { get; set; }
        
        // Service details
        public int ServiceId { get; set; }
        public Service Service { get; set; }
        
        // Pet details
        public int PetId { get; set; }
        public Pet Pet { get; set; }
        
        // Customer details
        public string CustomerId { get; set; }
        public ApplicationUser Customer { get; set; }
        
        // Provider details
        public string ProviderId { get; set; }
        public ApplicationUser Provider { get; set; }
        
        // Payment and tax details
        public decimal ServicePrice { get; set; }
        public decimal SubTotal { get; set; }
        public decimal GstAmount { get; set; }
        public decimal PstAmount { get; set; }
        public decimal HstAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsPaid { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
