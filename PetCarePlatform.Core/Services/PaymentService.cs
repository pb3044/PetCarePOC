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
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IBookingService _bookingService;
        private readonly IEmailService _emailService;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IPaymentRepository paymentRepository, 
            IBookingRepository bookingRepository, 
            IBookingService bookingService,
            IEmailService emailService,
            ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _bookingService = bookingService ?? throw new ArgumentNullException(nameof(bookingService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ============================================
        // Enterprise Pattern Methods (Result-based)
        // ============================================

        public async Task<Result<PaymentResponse>> GetPaymentByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting payment by ID: {PaymentId}", id);
                
                var payment = await _paymentRepository.GetByIdAsync(id).ConfigureAwait(false);
                if (payment == null)
                {
                    _logger.LogWarning("Payment not found: {PaymentId}", id);
                    return Result<PaymentResponse>.Failure("Payment not found", "PAYMENT_NOT_FOUND");
                }

                var response = MapToPaymentResponse(payment);
                return Result<PaymentResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment {PaymentId}", id);
                return Result<PaymentResponse>.Failure("An error occurred while retrieving the payment", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<PaymentResponse>> GetPaymentByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting payment by booking ID: {BookingId}", bookingId);
                
                var payment = await _paymentRepository.GetByBookingIdAsync(bookingId).ConfigureAwait(false);
                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for booking: {BookingId}", bookingId);
                    return Result<PaymentResponse>.Failure("Payment not found for this booking", "PAYMENT_NOT_FOUND");
                }

                var response = MapToPaymentResponse(payment);
                return Result<PaymentResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment for booking {BookingId}", bookingId);
                return Result<PaymentResponse>.Failure("An error occurred while retrieving the payment", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<PaymentResponse>> GetPaymentByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting payment by transaction ID: {TransactionId}", transactionId);
                
                var payment = await _paymentRepository.GetByTransactionIdAsync(transactionId).ConfigureAwait(false);
                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for transaction: {TransactionId}", transactionId);
                    return Result<PaymentResponse>.Failure("Payment not found for this transaction", "PAYMENT_NOT_FOUND");
                }

                var response = MapToPaymentResponse(payment);
                return Result<PaymentResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment for transaction {TransactionId}", transactionId);
                return Result<PaymentResponse>.Failure("An error occurred while retrieving the payment", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<PagedResult<PaymentResponse>>> GetPaymentsAsync(PaymentQuery query, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting payments with query: UserId={UserId}, BookingId={BookingId}, Status={Status}", 
                    query.UserId, query.BookingId, query.Status);

                IEnumerable<Payment> payments;

                if (query.UserId.HasValue)
                {
                    payments = await _paymentRepository.GetByUserIdAsync(query.UserId.Value).ConfigureAwait(false);
                }
                else if (query.BookingId.HasValue)
                {
                    var payment = await _paymentRepository.GetByBookingIdAsync(query.BookingId.Value).ConfigureAwait(false);
                    payments = payment != null ? new[] { payment } : Enumerable.Empty<Payment>();
                }
                else
                {
                    payments = await _paymentRepository.GetAllAsync().ConfigureAwait(false);
                }

                // Apply filters
                if (query.Status.HasValue)
                {
                    payments = payments.Where(p => p.Status == query.Status.Value);
                }

                if (query.FromDate.HasValue)
                {
                    payments = payments.Where(p => p.CreatedAt >= query.FromDate.Value);
                }

                if (query.ToDate.HasValue)
                {
                    payments = payments.Where(p => p.CreatedAt <= query.ToDate.Value);
                }

                if (query.MinAmount.HasValue)
                {
                    payments = payments.Where(p => p.Amount >= query.MinAmount.Value);
                }

                if (query.MaxAmount.HasValue)
                {
                    payments = payments.Where(p => p.Amount <= query.MaxAmount.Value);
                }

                // Apply sorting
                payments = query.SortBy?.ToLower() switch
                {
                    "amount" => query.SortOrder == "asc" 
                        ? payments.OrderBy(p => p.Amount) 
                        : payments.OrderByDescending(p => p.Amount),
                    "createdat" => query.SortOrder == "asc" 
                        ? payments.OrderBy(p => p.CreatedAt) 
                        : payments.OrderByDescending(p => p.CreatedAt),
                    "status" => query.SortOrder == "asc" 
                        ? payments.OrderBy(p => p.Status) 
                        : payments.OrderByDescending(p => p.Status),
                    _ => payments.OrderByDescending(p => p.CreatedAt)
                };

                var totalCount = payments.Count();
                var items = payments
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(MapToPaymentResponse)
                    .ToList();

                var pagedResult = new PagedResult<PaymentResponse>(
                    items,
                    totalCount,
                    query.PageNumber,
                    query.PageSize
                );

                return Result<PagedResult<PaymentResponse>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payments");
                return Result<PagedResult<PaymentResponse>>.Failure("An error occurred while retrieving payments", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<PaymentResponse>> CreatePaymentIntentAsync(CreatePaymentIntentRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating payment intent for booking {BookingId}", request.BookingId);

                var booking = await _bookingRepository.GetByIdAsync(request.BookingId).ConfigureAwait(false);
                if (booking == null)
                {
                    _logger.LogWarning("Booking not found: {BookingId}", request.BookingId);
                    return Result<PaymentResponse>.Failure("Booking not found", "BOOKING_NOT_FOUND");
                }

                // Check if payment already exists
                var existingPayment = await _paymentRepository.GetByBookingIdAsync(request.BookingId).ConfigureAwait(false);
                if (existingPayment != null)
                {
                    _logger.LogWarning("Payment already exists for booking: {BookingId}", request.BookingId);
                    return Result<PaymentResponse>.Failure(
                        "Payment already exists for this booking", 
                        "PAYMENT_ALREADY_EXISTS");
                }

                // Calculate fees
                var platformFeeResult = await CalculatePlatformFeeAsync(booking.TotalPrice, cancellationToken).ConfigureAwait(false);
                if (platformFeeResult.IsFailure)
                {
                    _logger.LogWarning("Failed to calculate platform fee for booking {BookingId}", request.BookingId);
                    return Result<PaymentResponse>.Failure(platformFeeResult.ErrorMessage, platformFeeResult.ErrorCode);
                }
                var platformFee = platformFeeResult.Value;
                var providerPayout = booking.TotalPrice - platformFee;

                // Create a new payment
                var payment = new Payment
                {
                    BookingId = request.BookingId,
                    UserId = booking.OwnerId,
                    Amount = booking.TotalPrice,
                    PlatformFee = platformFee,
                    ProviderPayout = providerPayout,
                    Status = PaymentStatus.Pending,
                    Method = PaymentMethod.CreditCard, // Default, can be updated later
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdPayment = await _paymentRepository.CreateAsync(payment).ConfigureAwait(false);

                _logger.LogInformation("Payment intent created successfully: {PaymentId}", createdPayment.Id);

                var response = MapToPaymentResponse(createdPayment);
                return Result<PaymentResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment intent for booking {BookingId}", request.BookingId);
                return Result<PaymentResponse>.Failure("An error occurred while creating the payment intent", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<PaymentResponse>> ConfirmPaymentAsync(ConfirmPaymentRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Confirming payment {PaymentId} with transaction {TransactionId}", 
                    request.PaymentId, request.TransactionId);

                var payment = await _paymentRepository.GetByIdAsync(request.PaymentId).ConfigureAwait(false);
                if (payment == null)
                {
                    _logger.LogWarning("Payment not found: {PaymentId}", request.PaymentId);
                    return Result<PaymentResponse>.Failure("Payment not found", "PAYMENT_NOT_FOUND");
                }

                // Update payment details
                payment.TransactionId = request.TransactionId;
                payment.Status = PaymentStatus.Captured;
                payment.UpdatedAt = DateTime.UtcNow;

                await _paymentRepository.UpdateAsync(payment).ConfigureAwait(false);

                // Update booking status using the booking service
                var bookingResult = await _bookingService.ConfirmBookingAfterPaymentAsync(
                    payment.BookingId, 
                    payment.Id, 
                    cancellationToken).ConfigureAwait(false);

                if (bookingResult.IsFailure)
                {
                    _logger.LogWarning("Failed to confirm booking after payment: {Error}", bookingResult.ErrorMessage);
                    // Payment is confirmed, but booking confirmation failed - log and continue
                }

                // Send payment confirmation email
                try
                {
                    var booking = await _bookingRepository.GetByIdAsync(payment.BookingId).ConfigureAwait(false);
                    if (booking?.Owner?.User != null)
                    {
                        var petOwnerName = $"{booking.Owner.User.FirstName} {booking.Owner.User.LastName}";
                        var serviceName = booking.Service?.Title ?? "Service";
                        
                        await _emailService.SendPaymentConfirmationAsync(
                            booking.Owner.User.Email,
                            petOwnerName,
                            serviceName,
                            payment.Amount,
                            payment.Id
                        ).ConfigureAwait(false);
                        
                        _logger.LogInformation("Payment confirmation email sent for payment {PaymentId}", payment.Id);
                    }
                }
                catch (Exception emailEx)
                {
                    // Log email error but don't fail the payment confirmation
                    _logger.LogError(emailEx, "Failed to send payment confirmation email for payment {PaymentId}", payment.Id);
                }

                _logger.LogInformation("Payment confirmed successfully: {PaymentId}", request.PaymentId);

                var response = MapToPaymentResponse(payment);
                return Result<PaymentResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment {PaymentId}", request.PaymentId);
                return Result<PaymentResponse>.Failure("An error occurred while confirming the payment", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<PaymentResponse>> ProcessRefundAsync(ProcessRefundRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing refund for payment {PaymentId}, amount: {Amount}", 
                    request.PaymentId, request.Amount);

                var payment = await _paymentRepository.GetByIdAsync(request.PaymentId).ConfigureAwait(false);
                if (payment == null)
                {
                    _logger.LogWarning("Payment not found: {PaymentId}", request.PaymentId);
                    return Result<PaymentResponse>.Failure("Payment not found", "PAYMENT_NOT_FOUND");
                }

                if (payment.Status != PaymentStatus.Captured)
                {
                    _logger.LogWarning("Payment {PaymentId} cannot be refunded in status {Status}", 
                        request.PaymentId, payment.Status);
                    return Result<PaymentResponse>.Failure(
                        "Payment cannot be refunded in its current state", 
                        "PAYMENT_NOT_REFUNDABLE");
                }

                if (request.Amount > payment.Amount)
                {
                    _logger.LogWarning("Refund amount {Amount} exceeds payment amount {PaymentAmount} for payment {PaymentId}", 
                        request.Amount, payment.Amount, request.PaymentId);
                    return Result<PaymentResponse>.Failure(
                        "Refund amount cannot exceed the payment amount", 
                        "INVALID_REFUND_AMOUNT");
                }

                // Update payment status
                payment.Status = PaymentStatus.Refunded;
                payment.UpdatedAt = DateTime.UtcNow;

                await _paymentRepository.UpdateAsync(payment).ConfigureAwait(false);

                // Update booking status
                var booking = await _bookingRepository.GetByIdAsync(payment.BookingId).ConfigureAwait(false);
                if (booking != null)
                {
                    booking.Status = BookingStatus.Cancelled;
                    booking.Notes = (booking.Notes ?? string.Empty) + "\nRefund reason: " + request.Reason;
                    booking.UpdatedAt = DateTime.UtcNow;
                    await _bookingRepository.UpdateAsync(booking).ConfigureAwait(false);

                    // Send refund confirmation email
                    try
                    {
                        if (booking.Owner?.User != null)
                        {
                            var petOwnerName = $"{booking.Owner.User.FirstName} {booking.Owner.User.LastName}";
                            var serviceName = booking.Service?.Title ?? "Service";
                            
                            await _emailService.SendRefundConfirmationAsync(
                                booking.Owner.User.Email,
                                petOwnerName,
                                serviceName,
                                request.Amount,
                                request.Reason
                            ).ConfigureAwait(false);
                            
                            _logger.LogInformation("Refund confirmation email sent for payment {PaymentId}", payment.Id);
                        }
                    }
                    catch (Exception emailEx)
                    {
                        // Log email error but don't fail the refund processing
                        _logger.LogError(emailEx, "Failed to send refund confirmation email for payment {PaymentId}", payment.Id);
                    }
                }

                _logger.LogInformation("Refund processed successfully for payment {PaymentId}", request.PaymentId);

                var response = MapToPaymentResponse(payment);
                return Result<PaymentResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for payment {PaymentId}", request.PaymentId);
                return Result<PaymentResponse>.Failure("An error occurred while processing the refund", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<decimal>> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting total revenue from {StartDate} to {EndDate}", startDate, endDate);

                var revenue = await _paymentRepository.GetTotalRevenueAsync().ConfigureAwait(false);
                return Result<decimal>.Success(revenue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total revenue");
                return Result<decimal>.Failure("An error occurred while retrieving total revenue", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<decimal>> GetProviderEarningsAsync(int providerId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting provider earnings for provider {ProviderId}", providerId);

                var earnings = await _paymentRepository.GetProviderEarningsAsync(providerId).ConfigureAwait(false);
                return Result<decimal>.Success(earnings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provider earnings for provider {ProviderId}", providerId);
                return Result<decimal>.Failure("An error occurred while retrieving provider earnings", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<decimal>> CalculatePlatformFeeAsync(decimal amount, CancellationToken cancellationToken = default)
        {
            try
            {
                // Platform fee is 15% of the booking amount
                var fee = Math.Round(amount * 0.15m, 2);
                return Result<decimal>.Success(fee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating platform fee for amount {Amount}", amount);
                return Result<decimal>.Failure("An error occurred while calculating the platform fee", "INTERNAL_ERROR");
            }
        }

        public async Task<Result> UpdatePaymentStatusAsync(int paymentId, PaymentStatus status, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating payment status: {PaymentId} to {Status}", paymentId, status);
                
                var payment = await _paymentRepository.GetByIdAsync(paymentId).ConfigureAwait(false);
                if (payment == null)
                {
                    _logger.LogWarning("Payment not found: {PaymentId}", paymentId);
                    return Result.Failure("Payment not found", "PAYMENT_NOT_FOUND");
                }

                payment.Status = status;
                payment.UpdatedAt = DateTime.UtcNow;
                await _paymentRepository.UpdateAsync(payment).ConfigureAwait(false);
                
                _logger.LogInformation("Payment status updated: {PaymentId} to {Status}", paymentId, status);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status: {PaymentId}", paymentId);
                return Result.Failure("An error occurred while updating the payment status", "INTERNAL_ERROR");
            }
        }

        // Helper method to map Payment to PaymentResponse
        private PaymentResponse MapToPaymentResponse(Payment payment)
        {
            return new PaymentResponse
            {
                Id = payment.Id,
                BookingId = payment.BookingId,
                BookingServiceName = payment.Booking?.Service?.Title ?? "Unknown Service",
                UserId = payment.UserId,
                UserName = $"{payment.User?.FirstName} {payment.User?.LastName}".Trim(),
                Amount = payment.Amount,
                PlatformFee = payment.PlatformFee,
                ProviderPayout = payment.ProviderPayout,
                TransactionId = payment.TransactionId ?? string.Empty,
                Status = payment.Status,
                Method = payment.Method,
                ReceiptUrl = payment.ReceiptUrl,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            };
        }
    }
}
