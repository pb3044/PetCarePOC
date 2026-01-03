using System.Threading.Tasks;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task SendBookingConfirmationAsync(string to, string petOwnerName, string serviceName, DateTime serviceDate, decimal amount);
        Task SendBookingCancellationAsync(string to, string petOwnerName, string serviceName, DateTime serviceDate);
        Task SendPaymentConfirmationAsync(string to, string petOwnerName, string serviceName, decimal amount, int paymentId);
        Task SendRefundConfirmationAsync(string to, string petOwnerName, string serviceName, decimal refundAmount, string reason);
        Task SendReviewRequestAsync(string to, string petOwnerName, string serviceName, int bookingId);
    }
}
