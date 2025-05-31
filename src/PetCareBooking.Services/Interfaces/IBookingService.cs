using PetCareBooking.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetCareBooking.Services.Interfaces
{
    public interface IBookingService
    {
        Task<Booking> CreateBookingAsync(Booking booking);
        Task<Booking> GetBookingByIdAsync(int id);
        Task<IEnumerable<Booking>> GetBookingsByCustomerIdAsync(string customerId);
        Task<IEnumerable<Booking>> GetBookingsByProviderIdAsync(string providerId);
        Task<IEnumerable<Booking>> GetBookingsByPetIdAsync(int petId);
        Task<Booking> UpdateBookingStatusAsync(int id, string status);
        Task<bool> DeleteBookingAsync(int id);
        Task<Booking> CalculateBookingTaxesAsync(Booking booking, string provinceCode);
    }
}
