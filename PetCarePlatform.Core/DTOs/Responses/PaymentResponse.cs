using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.DTOs.Responses
{
    public class PaymentResponse
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string BookingServiceName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal ProviderPayout { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public PaymentMethod Method { get; set; }
        public string? ReceiptUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

