using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment> GetByIdAsync(int id);
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<Payment> GetByBookingIdAsync(int bookingId);
        Task<IEnumerable<Payment>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status);
        Task<Payment> CreateAsync(Payment payment);
        Task UpdateAsync(Payment payment);
        Task UpdateStatusAsync(int id, PaymentStatus status);
        Task DeleteAsync(int id);
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetProviderEarningsAsync(int providerId);
    }
}
