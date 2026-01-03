namespace PetCarePlatform.Core.DTOs.Requests
{
    public class ProcessRefundRequest
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}

