using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking> GetByIdAsync(int id);
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<IEnumerable<Booking>> GetByOwnerIdAsync(int ownerId);
        Task<IEnumerable<Booking>> GetByProviderIdAsync(int providerId);
        Task<IEnumerable<Booking>> GetByServiceIdAsync(int serviceId);
        Task<IEnumerable<Booking>> GetByStatusAsync(BookingStatus status);
        Task<IEnumerable<Booking>> GetUpcomingBookingsAsync(int userId);
        Task<Booking> CreateAsync(Booking booking);
        Task UpdateAsync(Booking booking);
        Task UpdateStatusAsync(int id, BookingStatus status);
        Task DeleteAsync(int id);
        Task<bool> IsTimeSlotAvailableAsync(int serviceId, DateTime startTime, DateTime endTime);
    }
}
