using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PetCarePlatform.Core.Common;
using PetCarePlatform.Core.DTOs.Queries;
using PetCarePlatform.Core.DTOs.Requests;
using PetCarePlatform.Core.DTOs.Responses;
using PetCarePlatform.Core.Exceptions;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IPetRepository _petRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<BookingService> _logger;
        
        public BookingService(
            IBookingRepository bookingRepository,
            IServiceRepository serviceRepository,
            IPetRepository petRepository,
            IEmailService emailService,
            ILogger<BookingService> logger)
        {
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            _petRepository = petRepository ?? throw new ArgumentNullException(nameof(petRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Booking>> GetBookingsByOwnerIdAsync(int ownerId, CancellationToken cancellationToken = default)
        {
            return await _bookingRepository.GetByOwnerIdAsync(ownerId).ConfigureAwait(false);
        }

        public async Task<IEnumerable<Booking>> GetUpcomingBookingsAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _bookingRepository.GetUpcomingBookingsAsync(userId).ConfigureAwait(false);
        }

        public async Task<Booking> CreateBookingAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating booking for service {ServiceId}", booking.ServiceId);

            // Validate service exists
            var service = await _serviceRepository.GetByIdAsync(booking.ServiceId).ConfigureAwait(false);
            if (service == null)
            {
                _logger.LogWarning("Service not found: {ServiceId}", booking.ServiceId);
                throw new EntityNotFoundException("Service", booking.ServiceId);
            }

            // Validate pet exists if specified
            if (booking.PetId.HasValue)
            {
                var pet = await _petRepository.GetByIdAsync(booking.PetId.Value).ConfigureAwait(false);
                if (pet == null)
                {
                    _logger.LogWarning("Pet not found: {PetId}", booking.PetId.Value);
                    throw new EntityNotFoundException("Pet", booking.PetId.Value);
                }
            }

            // Check if time slot is available
            if (!await IsTimeSlotAvailableAsync(booking.ServiceId, booking.StartTime, booking.EndTime, cancellationToken).ConfigureAwait(false))
            {
                _logger.LogWarning("Time slot not available for service {ServiceId} from {StartTime} to {EndTime}", 
                    booking.ServiceId, booking.StartTime, booking.EndTime);
                throw new BusinessRuleViolationException("TimeSlotAvailability", 
                    "The selected time slot is not available");
            }

            // Calculate price if not already set
            if (booking.TotalPrice == 0)
            {
                booking.TotalPrice = await CalculateBookingPriceAsync(
                    booking.ServiceId, 
                    booking.StartTime, 
                    booking.EndTime, 
                    booking.PetId ?? 0,
                    cancellationToken).ConfigureAwait(false);
            }

            // Set default values
            booking.Status = BookingStatus.Requested;
            booking.CreatedAt = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;

            return await _bookingRepository.CreateAsync(booking).ConfigureAwait(false);
        }

        public async Task<bool> IsTimeSlotAvailableAsync(int serviceId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
        {
            return await _bookingRepository.IsTimeSlotAvailableAsync(serviceId, startTime, endTime).ConfigureAwait(false);
        }

        public async Task<bool> IsTimeSlotAvailableAsync(int serviceId, DateTime startTime, DateTime endTime, int excludeBookingId, CancellationToken cancellationToken = default)
        {
            return await _bookingRepository.IsTimeSlotAvailableAsync(serviceId, startTime, endTime, excludeBookingId).ConfigureAwait(false);
        }

        public async Task<Result> UpdateBookingStatusAsync(int id, BookingStatus status, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating booking status: {BookingId} to {Status}", id, status);
                
                var booking = await _bookingRepository.GetByIdAsync(id).ConfigureAwait(false);
                if (booking == null)
                {
                    _logger.LogWarning("Booking not found: {BookingId}", id);
                    return Result.Failure("Booking not found", "BOOKING_NOT_FOUND");
                }

                await _bookingRepository.UpdateStatusAsync(id, status).ConfigureAwait(false);
                _logger.LogInformation("Booking status updated: {BookingId} to {Status}", id, status);
                
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking status: {BookingId}", id);
                return Result.Failure("An error occurred while updating the booking status", "INTERNAL_ERROR");
            }
        }

        public async Task UpdateBookingAsync(Booking booking, CancellationToken cancellationToken = default)
        {
            var existingBooking = await _bookingRepository.GetByIdAsync(booking.Id).ConfigureAwait(false);
            if (existingBooking == null)
            {
                _logger.LogWarning("Booking not found: {BookingId}", booking.Id);
                throw new EntityNotFoundException("Booking", booking.Id);
            }

            // Check if time slot is available (excluding current booking)
            if (!await IsTimeSlotAvailableAsync(booking.ServiceId, booking.StartTime, booking.EndTime, booking.Id, cancellationToken).ConfigureAwait(false))
            {
                _logger.LogWarning("Time slot not available for booking update {BookingId}", booking.Id);
                throw new BusinessRuleViolationException("TimeSlotAvailability", 
                    "The selected time slot is not available");
            }

            // Update the booking
            existingBooking.StartTime = booking.StartTime;
            existingBooking.EndTime = booking.EndTime;
            existingBooking.SpecialInstructions = booking.SpecialInstructions;
            existingBooking.UpdatedAt = DateTime.UtcNow;

            await _bookingRepository.UpdateAsync(existingBooking).ConfigureAwait(false);
        }

        public async Task CancelBookingAsync(int id, string cancellationReason, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Cancelling booking {BookingId}", id);
            
            var booking = await _bookingRepository.GetByIdAsync(id).ConfigureAwait(false);
            if (booking == null)
            {
                _logger.LogWarning("Booking not found for cancellation: {BookingId}", id);
                throw new EntityNotFoundException("Booking", id);
            }

            // Check if booking can be cancelled
            if (booking.Status != BookingStatus.Requested && booking.Status != BookingStatus.Confirmed)
            {
                _logger.LogWarning("Booking {BookingId} cannot be cancelled in status {Status}", id, booking.Status);
                throw new BusinessRuleViolationException("BookingCancellation", 
                    "Booking cannot be cancelled in its current state");
            }

            booking.Status = BookingStatus.Cancelled;
            booking.Notes = booking.Notes + "\nCancellation reason: " + cancellationReason;
            booking.UpdatedAt = DateTime.UtcNow;

            await _bookingRepository.UpdateAsync(booking).ConfigureAwait(false);
        }

        public async Task<Result<bool>> CanBeReviewedAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId).ConfigureAwait(false);
            if (booking == null)
            {
                return Result<bool>.Failure("Booking not found");
            }

            // Booking can be reviewed only if it's completed
            return Result<bool>.Success(booking.Status == BookingStatus.Completed);
        }

        public async Task<decimal> CalculateBookingPriceAsync(int serviceId, DateTime startTime, DateTime endTime, int petId, CancellationToken cancellationToken = default)
        {
            var service = await _serviceRepository.GetByIdAsync(serviceId).ConfigureAwait(false);
            if (service == null)
            {
                _logger.LogWarning("Service not found for price calculation: {ServiceId}", serviceId);
                throw new EntityNotFoundException("Service", serviceId);
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

        public async Task<IEnumerable<Booking>> GetBookingsByStatusAsync(BookingStatus status, CancellationToken cancellationToken = default)
        {
            return await _bookingRepository.GetByStatusAsync(status).ConfigureAwait(false);
        }

        public async Task<bool> CanCancelBookingAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId).ConfigureAwait(false);
            if (booking == null)
            {
                return false;
            }

            // Can cancel if booking is requested or confirmed, and not within 24 hours of start time
            var canCancel = (booking.Status == BookingStatus.Requested || booking.Status == BookingStatus.Confirmed) &&
                           booking.StartTime > DateTime.UtcNow.AddHours(24);

            return canCancel;
        }

        // ============================================
        // Enterprise Pattern Methods (Result-based)
        // ============================================

        public async Task<Result<BookingResponse>> GetBookingByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting booking by ID: {BookingId}", id);
                
                var booking = await _bookingRepository.GetByIdAsync(id).ConfigureAwait(false);
                if (booking == null)
                {
                    _logger.LogWarning("Booking not found: {BookingId}", id);
                    return Result<BookingResponse>.Failure("Booking not found", "BOOKING_NOT_FOUND");
                }

                var response = MapToBookingResponse(booking);
                return Result<BookingResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking {BookingId}", id);
                return Result<BookingResponse>.Failure("An error occurred while retrieving the booking", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<PagedResult<BookingResponse>>> GetBookingsAsync(BookingQuery query, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting bookings with query: OwnerId={OwnerId}, ProviderId={ProviderId}, Status={Status}", 
                    query.OwnerId, query.ProviderId, query.Status);

                IEnumerable<Booking> bookings;

                if (query.OwnerId.HasValue)
                {
                    bookings = await _bookingRepository.GetByOwnerIdAsync(query.OwnerId.Value).ConfigureAwait(false);
                }
                else if (query.ProviderId.HasValue)
                {
                    bookings = await _bookingRepository.GetByProviderIdAsync(query.ProviderId.Value).ConfigureAwait(false);
                }
                else if (query.ServiceId.HasValue)
                {
                    bookings = await _bookingRepository.GetByServiceIdAsync(query.ServiceId.Value).ConfigureAwait(false);
                }
                else
                {
                    bookings = await _bookingRepository.GetAllAsync().ConfigureAwait(false);
                }

                // Apply filters
                if (query.Status.HasValue)
                {
                    bookings = bookings.Where(b => b.Status == query.Status.Value);
                }

                if (query.FromDate.HasValue)
                {
                    bookings = bookings.Where(b => b.StartTime >= query.FromDate.Value);
                }

                if (query.ToDate.HasValue)
                {
                    bookings = bookings.Where(b => b.StartTime <= query.ToDate.Value);
                }

                if (query.UpcomingOnly == true)
                {
                    bookings = bookings.Where(b => b.StartTime > DateTime.UtcNow);
                }

                // Apply sorting
                bookings = query.SortBy?.ToLower() switch
                {
                    "starttime" => query.SortOrder == "asc" 
                        ? bookings.OrderBy(b => b.StartTime) 
                        : bookings.OrderByDescending(b => b.StartTime),
                    "createdat" => query.SortOrder == "asc" 
                        ? bookings.OrderBy(b => b.CreatedAt) 
                        : bookings.OrderByDescending(b => b.CreatedAt),
                    "totalprice" => query.SortOrder == "asc" 
                        ? bookings.OrderBy(b => b.TotalPrice) 
                        : bookings.OrderByDescending(b => b.TotalPrice),
                    _ => bookings.OrderBy(b => b.StartTime)
                };

                var totalCount = bookings.Count();
                var items = bookings
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(MapToBookingResponse)
                    .ToList();

                var pagedResult = new PagedResult<BookingResponse>(
                    items,
                    totalCount,
                    query.PageNumber,
                    query.PageSize
                );

                return Result<PagedResult<BookingResponse>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings");
                return Result<PagedResult<BookingResponse>>.Failure("An error occurred while retrieving bookings", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<BookingResponse>> CreateBookingAsync(CreateBookingRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating booking for service {ServiceId} by owner {OwnerId}", 
                    request.ServiceId, request.OwnerId);

                // Validate service exists
                var service = await _serviceRepository.GetByIdAsync(request.ServiceId).ConfigureAwait(false);
                if (service == null)
                {
                    _logger.LogWarning("Service not found: {ServiceId}", request.ServiceId);
                    return Result<BookingResponse>.Failure("Service not found", "SERVICE_NOT_FOUND");
                }

                // Validate pet exists if specified
                if (request.PetId.HasValue)
                {
                    var pet = await _petRepository.GetByIdAsync(request.PetId.Value).ConfigureAwait(false);
                    if (pet == null)
                    {
                        _logger.LogWarning("Pet not found: {PetId}", request.PetId.Value);
                        return Result<BookingResponse>.Failure("Pet not found", "PET_NOT_FOUND");
                    }
                }

                // Check if time slot is available
                var availabilityResult = await IsTimeSlotAvailableAsync(
                    request.ServiceId, 
                    request.StartTime, 
                    request.EndTime, 
                    (int?)null, 
                    cancellationToken).ConfigureAwait(false);
                
                if (availabilityResult.IsFailure || !availabilityResult.Value)
                {
                    _logger.LogWarning("Time slot not available for service {ServiceId} from {StartTime} to {EndTime}", 
                        request.ServiceId, request.StartTime, request.EndTime);
                    return Result<BookingResponse>.Failure("The selected time slot is not available", "TIME_SLOT_UNAVAILABLE");
                }

                // Calculate price if not provided
                decimal totalPrice = request.TotalPrice ?? 0;
                if (totalPrice == 0)
                {
                    var priceResult = await CalculateBookingPriceAsync(
                        request.ServiceId, 
                        request.StartTime, 
                        request.EndTime, 
                        request.PetId, 
                        cancellationToken).ConfigureAwait(false);
                    
                    if (priceResult.IsFailure)
                    {
                        return Result<BookingResponse>.Failure(priceResult.ErrorMessage, priceResult.ErrorCode);
                    }
                    
                    totalPrice = priceResult.Value;
                }

                // Create booking
                var booking = new Booking
                {
                    ServiceId = request.ServiceId,
                    OwnerId = request.OwnerId,
                    PetId = request.PetId,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    TotalPrice = totalPrice,
                    SpecialInstructions = request.SpecialInstructions ?? string.Empty,
                    Status = BookingStatus.Requested,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdBooking = await _bookingRepository.CreateAsync(booking).ConfigureAwait(false);

                _logger.LogInformation("Booking created successfully: {BookingId}", createdBooking.Id);

                var response = MapToBookingResponse(createdBooking);
                return Result<BookingResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking for service {ServiceId}", request.ServiceId);
                return Result<BookingResponse>.Failure("An error occurred while creating the booking", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<BookingResponse>> UpdateBookingAsync(UpdateBookingRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating booking {BookingId}", request.BookingId);

                var existingBooking = await _bookingRepository.GetByIdAsync(request.BookingId).ConfigureAwait(false);
                if (existingBooking == null)
                {
                    _logger.LogWarning("Booking not found: {BookingId}", request.BookingId);
                    return Result<BookingResponse>.Failure("Booking not found", "BOOKING_NOT_FOUND");
                }

                // Check if booking can be updated (only requested or confirmed bookings can be updated)
                if (existingBooking.Status != BookingStatus.Requested && existingBooking.Status != BookingStatus.Confirmed)
                {
                    _logger.LogWarning("Booking {BookingId} cannot be updated in status {Status}", 
                        request.BookingId, existingBooking.Status);
                    return Result<BookingResponse>.Failure(
                        "Booking cannot be updated in its current state", 
                        "BOOKING_NOT_UPDATABLE");
                }

                // Check if time slot is available (excluding current booking)
                var availabilityResult = await IsTimeSlotAvailableAsync(
                    existingBooking.ServiceId, 
                    request.StartTime, 
                    request.EndTime, 
                    (int?)request.BookingId, 
                    cancellationToken).ConfigureAwait(false);
                
                if (availabilityResult.IsFailure || !availabilityResult.Value)
                {
                    _logger.LogWarning("Time slot not available for booking update {BookingId}", request.BookingId);
                    return Result<BookingResponse>.Failure("The selected time slot is not available", "TIME_SLOT_UNAVAILABLE");
                }

                // Update the booking
                existingBooking.StartTime = request.StartTime;
                existingBooking.EndTime = request.EndTime;
                existingBooking.SpecialInstructions = request.SpecialInstructions ?? string.Empty;
                existingBooking.UpdatedAt = DateTime.UtcNow;

                await _bookingRepository.UpdateAsync(existingBooking).ConfigureAwait(false);

                _logger.LogInformation("Booking updated successfully: {BookingId}", request.BookingId);

                var response = MapToBookingResponse(existingBooking);
                return Result<BookingResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking {BookingId}", request.BookingId);
                return Result<BookingResponse>.Failure("An error occurred while updating the booking", "INTERNAL_ERROR");
            }
        }

        public async Task<Result> CancelBookingAsync(CancelBookingRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Cancelling booking {BookingId}", request.BookingId);
                
                var booking = await _bookingRepository.GetByIdAsync(request.BookingId).ConfigureAwait(false);
                if (booking == null)
                {
                    _logger.LogWarning("Booking not found for cancellation: {BookingId}", request.BookingId);
                    return Result.Failure("Booking not found", "BOOKING_NOT_FOUND");
                }

                // Check if booking can be cancelled
                if (booking.Status != BookingStatus.Requested && booking.Status != BookingStatus.Confirmed)
                {
                    _logger.LogWarning("Booking {BookingId} cannot be cancelled in status {Status}", 
                        request.BookingId, booking.Status);
                    return Result.Failure(
                        "Booking cannot be cancelled in its current state", 
                        "BOOKING_NOT_CANCELLABLE");
                }

                booking.Status = BookingStatus.Cancelled;
                booking.Notes = (booking.Notes ?? string.Empty) + "\nCancellation reason: " + request.CancellationReason;
                booking.UpdatedAt = DateTime.UtcNow;

                await _bookingRepository.UpdateAsync(booking).ConfigureAwait(false);

                _logger.LogInformation("Booking cancelled successfully: {BookingId}", request.BookingId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId}", request.BookingId);
                return Result.Failure("An error occurred while cancelling the booking", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<BookingResponse>> ConfirmBookingAfterPaymentAsync(int bookingId, int paymentId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Confirming booking {BookingId} after payment {PaymentId}", bookingId, paymentId);
                
                var booking = await _bookingRepository.GetByIdAsync(bookingId).ConfigureAwait(false);
                if (booking == null)
                {
                    _logger.LogWarning("Booking not found for confirmation: {BookingId}", bookingId);
                    return Result<BookingResponse>.Failure("Booking not found", "BOOKING_NOT_FOUND");
                }

                // Update booking with payment reference and confirm
                booking.PaymentId = paymentId;
                booking.Status = BookingStatus.Confirmed;
                booking.UpdatedAt = DateTime.UtcNow;

                await _bookingRepository.UpdateAsync(booking).ConfigureAwait(false);

                // Send confirmation email
                try
                {
                    var petOwnerName = $"{booking.Owner?.User?.FirstName} {booking.Owner?.User?.LastName}".Trim();
                    var serviceName = booking.Service?.Title ?? "Service";
                    var serviceDate = booking.StartTime;
                    var amount = booking.TotalPrice;

                    await _emailService.SendBookingConfirmationAsync(
                        booking.Owner?.User?.Email ?? "",
                        petOwnerName,
                        serviceName,
                        serviceDate,
                        amount
                    ).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // Log email error but don't fail the booking confirmation
                    _logger.LogError(ex, "Failed to send confirmation email for booking {BookingId}", bookingId);
                }

                _logger.LogInformation("Booking confirmed successfully: {BookingId}", bookingId);

                var response = MapToBookingResponse(booking);
                return Result<BookingResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming booking {BookingId}", bookingId);
                return Result<BookingResponse>.Failure("An error occurred while confirming the booking", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<bool>> IsTimeSlotAvailableAsync(int serviceId, DateTime startTime, DateTime endTime, int? excludeBookingId = null, CancellationToken cancellationToken = default)
        {
            try
            {
                bool isAvailable;
                if (excludeBookingId.HasValue)
                {
                    isAvailable = await _bookingRepository.IsTimeSlotAvailableAsync(
                        serviceId, startTime, endTime, excludeBookingId.Value).ConfigureAwait(false);
                }
                else
                {
                    isAvailable = await _bookingRepository.IsTimeSlotAvailableAsync(
                        serviceId, startTime, endTime).ConfigureAwait(false);
                }

                return Result<bool>.Success(isAvailable);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking time slot availability for service {ServiceId}", serviceId);
                return Result<bool>.Failure("An error occurred while checking time slot availability", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<decimal>> CalculateBookingPriceAsync(int serviceId, DateTime startTime, DateTime endTime, int? petId = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var service = await _serviceRepository.GetByIdAsync(serviceId).ConfigureAwait(false);
                if (service == null)
                {
                    _logger.LogWarning("Service not found for price calculation: {ServiceId}", serviceId);
                    return Result<decimal>.Failure("Service not found", "SERVICE_NOT_FOUND");
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

                return Result<decimal>.Success(totalPrice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating booking price for service {ServiceId}", serviceId);
                return Result<decimal>.Failure("An error occurred while calculating the booking price", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<IEnumerable<BookingResponse>>> GetBookingsByProviderIdAsync(int providerId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting bookings by provider ID: {ProviderId}", providerId);
                
                var bookings = await _bookingRepository.GetByProviderIdAsync(providerId).ConfigureAwait(false);
                var responses = bookings.Select(MapToBookingResponse).ToList();
                
                return Result<IEnumerable<BookingResponse>>.Success(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for provider {ProviderId}", providerId);
                return Result<IEnumerable<BookingResponse>>.Failure("An error occurred while retrieving bookings", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<IEnumerable<BookingResponse>>> GetBookingsByServiceIdAsync(int serviceId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting bookings by service ID: {ServiceId}", serviceId);
                
                var bookings = await _bookingRepository.GetByServiceIdAsync(serviceId).ConfigureAwait(false);
                var responses = bookings.Select(MapToBookingResponse).ToList();
                
                return Result<IEnumerable<BookingResponse>>.Success(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for service {ServiceId}", serviceId);
                return Result<IEnumerable<BookingResponse>>.Failure("An error occurred while retrieving bookings", "INTERNAL_ERROR");
            }
        }

        // Helper method to map Booking to BookingResponse
        private BookingResponse MapToBookingResponse(Booking booking)
        {
            var canCancel = booking.Status == BookingStatus.Requested || 
                           booking.Status == BookingStatus.Confirmed;
            var canReview = booking.Status == BookingStatus.Completed;

            return new BookingResponse
            {
                Id = booking.Id,
                ServiceId = booking.ServiceId,
                ServiceName = booking.Service?.Title ?? "Unknown Service",
                OwnerId = booking.OwnerId,
                OwnerName = $"{booking.Owner?.User?.FirstName} {booking.Owner?.User?.LastName}".Trim(),
                PetId = booking.PetId,
                PetName = booking.Pet?.Name,
                StartTime = booking.StartTime,
                EndTime = booking.EndTime,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                SpecialInstructions = booking.SpecialInstructions,
                Notes = booking.Notes,
                PaymentId = booking.PaymentId,
                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt,
                CanCancel = canCancel,
                CanReview = canReview
            };
        }
    }
}
