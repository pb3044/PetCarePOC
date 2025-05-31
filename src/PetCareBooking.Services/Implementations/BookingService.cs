using Microsoft.EntityFrameworkCore;
using PetCareBooking.Core.Models;
using PetCareBooking.Infrastructure.Data;
using PetCareBooking.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetCareBooking.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITaxService _taxService;

        public BookingService(ApplicationDbContext context, ITaxService taxService)
        {
            _context = context;
            _taxService = taxService;
        }

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            if (booking == null)
            {
                throw new ArgumentNullException(nameof(booking));
            }

            booking.CreatedAt = DateTime.UtcNow;
            booking.Status = "Pending";

            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();

            return booking;
        }

        public async Task<Booking> GetBookingByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.Pet)
                .Include(b => b.Service)
                .Include(b => b.Customer)
                .Include(b => b.Provider)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByCustomerIdAsync(string customerId)
        {
            return await _context.Bookings
                .Include(b => b.Pet)
                .Include(b => b.Service)
                .Include(b => b.Provider)
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByProviderIdAsync(string providerId)
        {
            return await _context.Bookings
                .Include(b => b.Pet)
                .Include(b => b.Service)
                .Include(b => b.Customer)
                .Where(b => b.ProviderId == providerId)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByPetIdAsync(int petId)
        {
            return await _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.Provider)
                .Where(b => b.PetId == petId)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<Booking> UpdateBookingStatusAsync(int id, string status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return null;
            }

            booking.Status = status;
            booking.UpdatedAt = DateTime.UtcNow;

            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            return booking;
        }

        public async Task<bool> DeleteBookingAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return false;
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Booking> CalculateBookingTaxesAsync(Booking booking, string provinceCode)
        {
            if (booking == null)
            {
                throw new ArgumentNullException(nameof(booking));
            }

            if (string.IsNullOrEmpty(provinceCode))
            {
                throw new ArgumentException("Province code cannot be null or empty", nameof(provinceCode));
            }

            // Get the service price
            booking.ServicePrice = booking.Service?.BasePrice ?? booking.ServicePrice;
            booking.SubTotal = booking.ServicePrice;

            // Calculate taxes
            var taxResult = await _taxService.CalculateTaxAsync(booking.SubTotal, provinceCode);

            // Update booking with tax information
            booking.GstAmount = taxResult.GstAmount;
            booking.PstAmount = taxResult.PstAmount;
            booking.HstAmount = taxResult.HstAmount;
            booking.TotalAmount = taxResult.TotalAmount;

            return booking;
        }
    }
}
