namespace PetCarePlatform.Core.DTOs.Requests
{
    public class ConfirmPaymentRequest
    {
        public int PaymentId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
    }
}

