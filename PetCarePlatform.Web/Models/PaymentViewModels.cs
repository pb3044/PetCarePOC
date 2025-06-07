using System.ComponentModel.DataAnnotations;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Web.Models
{
    public class ProcessPaymentViewModel
    {
        public int BookingId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        
        public string ServiceName { get; set; }
        
        [Required]
        public PaymentMethod Method { get; set; }
    }

    public class PaymentDetailsViewModel
    {
        public Payment Payment { get; set; }
        public Booking Booking { get; set; }
        public bool CanRequestRefund { get; set; }
    }

    public class RefundRequestViewModel
    {
        public int PaymentId { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Reason { get; set; }
        
        public decimal RefundAmount { get; set; }
    }
}

