using System;
using System.Collections.Generic;

namespace PetCarePlatform.Core.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal ProviderPayout { get; set; }
        public string TransactionId { get; set; }
        public PaymentStatus Status { get; set; }
        public PaymentMethod Method { get; set; }
        public string ReceiptUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual Booking Booking { get; set; }
        public virtual User User { get; set; }
    }

    public enum PaymentStatus
    {
        Pending,
        Authorized,
        Captured,
        Refunded,
        Failed,
        Cancelled
    }

    public enum PaymentMethod
    {
        CreditCard,
        DebitCard,
        PayPal,
        BankTransfer,
        ApplePay,
        GooglePay
    }
}
