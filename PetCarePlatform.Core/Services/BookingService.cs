using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IPetRepository _petRepository;
        
        public BookingService(
            IBookingRepository bookingRepository,
            IServiceRepository serviceRepository,
            IPetRepository petRepository)
        {
            _bookingRepository = bookingRepository;
            _serviceRepository = serviceRepository;
            _petRepository = petRepository;
        }

        public async Task<Booking> GetBookingByIdAsync(int id)
        {
            return await _bookingRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByOwnerIdAsync(int ownerId)
        {
            return await _bookingRepository.GetByOwnerIdAsync(ownerId);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByProviderIdAsync(int providerId)
        {
            return await _bookingRepository.GetByProviderIdAsync(providerId);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByServiceIdAsync(int serviceId)
        {
            return await _bookingRepository.GetByServiceIdAsync(serviceId);
        }

        public async Task<IEnumerable<Booking>> GetUpcomingBookingsAsync(int userId)
        {
            return await _bookingRepository.GetUpcomingBookingsAsync(userId);
        }

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            // Validate service exists
            var service = await _serviceRepository.GetByIdAsync(booking.ServiceId);
            if (service == null)
            {
                throw new InvalidOperationException("Service not found");
            }

            // Validate pet exists if specified
            if (booking.PetId.HasValue)
            {
                var pet = await _petRepository.GetByIdAsync(booking.PetId.Value);
                if (pet == null)
                {
                    throw new InvalidOperationException("Pet not found");
                }
            }

            // Check if time slot is available
            if (!await IsTimeSlotAvailableAsync(booking.ServiceId, booking.StartTime, booking.EndTime))
            {
                throw new InvalidOperationException("The selected time slot is not available");
            }

            // Calculate price
            booking.TotalPrice = await CalculateBookingPriceAsync(
                booking.ServiceId, 
                booking.StartTime, 
                booking.EndTime, 
                booking.PetId ?? 0);

            // Set default values
            booking.Status = BookingStatus.Requested;
            booking.CreatedAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;

            return await _bookingRepository.CreateAsync(booking);
        }

        public async Task<bool> IsTimeSlotAvailableAsync(int serviceId, DateTime startTime, DateTime endTime)
        {
            return await _bookingRepository.IsTimeSlotAvailableAsync(serviceId, startTime, endTime);
        }

        public async Task UpdateBookingStatusAsync(int id, BookingStatus status)
        {
            await _bookingRepository.UpdateStatusAsync(id, status);
        }

        public async Task CancelBookingAsync(int id, string cancellationReason)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                throw new InvalidOperationException("Booking not found");
            }

            // Check if booking can be cancelled
            if (booking.Status != BookingStatus.Requested && booking.Status != BookingStatus.Confirmed)
            {
                throw new InvalidOperationException("Booking cannot be cancelled in its current state");
            }

            booking.Status = BookingStatus.Cancelled;
            booking.Notes = booking.Notes + "\nCancellation reason: " + cancellationReason;
            booking.UpdatedAt = DateTime.UtcNow;

            await _bookingRepository.UpdateAsync(booking);
        }

        public async Task<bool> CanBeReviewedAsync(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return false;
            }

            // Booking can be reviewed only if it's completed
            return booking.Status == BookingStatus.Completed;
        }

        public async Task<decimal> CalculateBookingPriceAsync(int serviceId, DateTime startTime, DateTime endTime, int petId)
        {
            var service = await _serviceRepository.GetByIdAsync(serviceId);
            if (service == null)
            {
                throw new InvalidOperationException("Service not found");
            }

            // Calculate duration based on price unit
            decimal totalPrice = 0;
            
            switch (service.PriceUnit)
            {
                case "per hour":
                    var hours = (decimal)(endTime - startTime).TotalHours;
                    totalPrice = service.BasePrice * hours;
                    break;
                case "per day":
                    var days = (decimal)Math.Ceiling((endTime - startTime).TotalDays);
                    totalPrice = service.BasePrice * days;
                    break;
                case "per visit":
                    totalPrice = service.BasePrice;
                    break;
                default:
                    totalPrice = service.BasePrice;
                    break;
            }

            // Additional logic for pet-specific pricing could be added here
            // For example, larger pets might cost more

            return totalPrice;
        }
    }
}
