using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PetCarePlatform.Core.Common;
using PetCarePlatform.Core.DTOs.Queries;
using PetCarePlatform.Core.DTOs.Requests;
using PetCarePlatform.Core.DTOs.Responses;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IBookingService
    {
        Task<Result<BookingResponse>> GetBookingByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<PagedResult<BookingResponse>>> GetBookingsAsync(BookingQuery query, CancellationToken cancellationToken = default);
        Task<Result<BookingResponse>> CreateBookingAsync(CreateBookingRequest request, CancellationToken cancellationToken = default);
        Task<Result<BookingResponse>> UpdateBookingAsync(UpdateBookingRequest request, CancellationToken cancellationToken = default);
        Task<Result> CancelBookingAsync(CancelBookingRequest request, CancellationToken cancellationToken = default);
        Task<Result<BookingResponse>> ConfirmBookingAfterPaymentAsync(int bookingId, int paymentId, CancellationToken cancellationToken = default);
        Task<Result<bool>> IsTimeSlotAvailableAsync(int serviceId, DateTime startTime, DateTime endTime, int? excludeBookingId = null, CancellationToken cancellationToken = default);
        Task<Result<decimal>> CalculateBookingPriceAsync(int serviceId, DateTime startTime, DateTime endTime, int? petId = null, CancellationToken cancellationToken = default);
        Task<Result> UpdateBookingStatusAsync(int bookingId, BookingStatus status, CancellationToken cancellationToken = default);
        Task<Result<bool>> CanBeReviewedAsync(int bookingId, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<BookingResponse>>> GetBookingsByProviderIdAsync(int providerId, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<BookingResponse>>> GetBookingsByServiceIdAsync(int serviceId, CancellationToken cancellationToken = default);
    }
}
