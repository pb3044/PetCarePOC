using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IPaymentService
    {
        Task<Payment> GetPaymentByIdAsync(int id);
        Task<Payment> GetPaymentByBookingIdAsync(int bookingId);
        Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId);
        Task<Payment> CreatePaymentIntentAsync(int bookingId);
        Task<Payment> ConfirmPaymentAsync(int paymentId, string transactionId);
        Task<Payment> ProcessRefundAsync(int paymentId, decimal amount, string reason);
        Task UpdatePaymentStatusAsync(int paymentId, PaymentStatus status);
        Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetProviderEarningsAsync(int providerId, DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> CalculatePlatformFeeAsync(decimal amount);
    }
}
