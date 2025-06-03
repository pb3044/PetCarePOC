using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IBookingService
    {
        Task<Booking> GetBookingByIdAsync(int id);
        Task<IEnumerable<Booking>> GetBookingsByOwnerIdAsync(int ownerId);
        Task<IEnumerable<Booking>> GetBookingsByProviderIdAsync(int providerId);
        Task<IEnumerable<Booking>> GetBookingsByServiceIdAsync(int serviceId);
        Task<IEnumerable<Booking>> GetUpcomingBookingsAsync(int userId);
        Task<Booking> CreateBookingAsync(Booking booking);
        Task<bool> IsTimeSlotAvailableAsync(int serviceId, DateTime startTime, DateTime endTime);
        Task UpdateBookingStatusAsync(int id, BookingStatus status);
        Task CancelBookingAsync(int id, string cancellationReason);
        Task<bool> CanBeReviewedAsync(int bookingId);
        Task<decimal> CalculateBookingPriceAsync(int serviceId, DateTime startTime, DateTime endTime, int petId);
    }
}
