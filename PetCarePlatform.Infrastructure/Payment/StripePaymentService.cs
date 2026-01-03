using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Common;
using PetCarePlatform.Core.DTOs.Queries;
using PetCarePlatform.Core.DTOs.Requests;
using PetCarePlatform.Core.DTOs.Responses;
using PetCarePlatform.Core.Models;
using PaymentModel = PetCarePlatform.Core.Models.Payment;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PetCarePlatform.Infrastructure.Payment
{
    public class StripePaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(
            IConfiguration configuration,
            IBookingRepository bookingRepository,
            IPaymentRepository paymentRepository,
            IEmailService emailService,
            ILogger<StripePaymentService> logger)
        {
            _configuration = configuration;
            _apiKey = _configuration["Stripe:SecretKey"];
            _bookingRepository = bookingRepository;
            _paymentRepository = paymentRepository;
            _emailService = emailService;
            _logger = logger;
            
            // Initialize Stripe
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogError("Stripe SecretKey is not configured");
                throw new InvalidOperationException("Stripe SecretKey is not configured");
            }
            
            StripeConfiguration.ApiKey = _apiKey;
        }

        public async Task<Core.Models.Payment> GetPaymentByIdAsync(int id)
        {
            return await _paymentRepository.GetByIdAsync(id);
        }

        public async Task<Core.Models.Payment> GetPaymentByBookingIdAsync(int bookingId)
        {
            return await _paymentRepository.GetByBookingIdAsync(bookingId);
        }

        public async Task<Core.Models.Payment> GetPaymentByTransactionIdAsync(string transactionId)
        {
            return await _paymentRepository.GetByTransactionIdAsync(transactionId);
        }

        public async Task<IEnumerable<Core.Models.Payment>> GetPaymentsByUserIdAsync(int userId)
        {
            return await _paymentRepository.GetByUserIdAsync(userId);
        }

        public async Task<Core.Models.Payment> CreatePaymentIntentAsync(int bookingId)
        {
            try
            {
                _logger.LogInformation("Creating payment intent for booking {BookingId}", bookingId);

                var booking = await _bookingRepository.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    _logger.LogWarning("Booking {BookingId} not found", bookingId);
                    throw new InvalidOperationException("Booking not found");
                }

                // Check if payment already exists
                var existingPayment = await _paymentRepository.GetByBookingIdAsync(bookingId);
                if (existingPayment != null)
                {
                    _logger.LogWarning("Payment already exists for booking {BookingId}", bookingId);
                    throw new InvalidOperationException("Payment already exists for this booking");
                }

                // Validate booking amount
                if (booking.TotalPrice <= 0)
                {
                    _logger.LogWarning("Invalid booking amount {Amount} for booking {BookingId}", booking.TotalPrice, bookingId);
                    throw new InvalidOperationException("Invalid booking amount");
                }

                // Calculate platform fee
                decimal platformFee = await CalculatePlatformFeeAsync(booking.TotalPrice);
                decimal providerPayout = booking.TotalPrice - platformFee;

                // Create a payment intent with Stripe
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(booking.TotalPrice * 100), // Stripe uses cents
                    Currency = "cad",
                    Description = $"Payment for booking #{booking.Id}",
                    Metadata = new Dictionary<string, string>
                    {
                        { "BookingId", booking.Id.ToString() },
                        { "ServiceId", booking.ServiceId.ToString() },
                        { "OwnerId", booking.OwnerId.ToString() },
                        { "ProviderId", booking.Service.ProviderId.ToString() }
                    }
                };

                var service = new PaymentIntentService();
                var intent = await service.CreateAsync(options);

                _logger.LogInformation("Stripe payment intent created: {PaymentIntentId} for booking {BookingId}", intent.Id, bookingId);

                // Create payment record in our database
                var payment = new Core.Models.Payment
                {
                    BookingId = bookingId,
                    UserId = booking.OwnerId,
                    Amount = booking.TotalPrice,
                    PlatformFee = platformFee,
                    ProviderPayout = providerPayout,
                    TransactionId = intent.Id,
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdPayment = await _paymentRepository.CreateAsync(payment);
                _logger.LogInformation("Payment record created with ID {PaymentId} for booking {BookingId}", createdPayment.Id, bookingId);

                return createdPayment;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating payment intent for booking {BookingId}: {StripeError}", bookingId, ex.Message);
                throw new InvalidOperationException($"Payment service error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating payment intent for booking {BookingId}", bookingId);
                throw;
            }
        }

        public async Task<Core.Models.Payment> ConfirmPaymentAsync(int paymentId, string transactionId)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
            {
                throw new InvalidOperationException("Payment not found");
            }

            // Retrieve the payment intent from Stripe
            var service = new PaymentIntentService();
            var intent = await service.GetAsync(payment.TransactionId);

            // Check if the payment intent is successful
            if (intent.Status != "succeeded")
            {
                throw new InvalidOperationException($"Payment intent is not successful. Current status: {intent.Status}");
            }

            // Update payment status
            payment.Status = PaymentStatus.Captured;
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(payment);

            // Update booking status
            var booking = await _bookingRepository.GetByIdAsync(payment.BookingId);
            if (booking != null)
            {
                booking.PaymentId = paymentId;
                if (booking.Status == BookingStatus.Requested)
                {
                    booking.Status = BookingStatus.Confirmed;
                }
                booking.UpdatedAt = DateTime.UtcNow;
                await _bookingRepository.UpdateAsync(booking);

                // Send payment confirmation email
                try
                {
                    if (booking.Owner?.User != null)
                    {
                        var petOwnerName = $"{booking.Owner.User.FirstName} {booking.Owner.User.LastName}";
                        var serviceName = booking.Service?.Title ?? "Service";
                        
                        await _emailService.SendPaymentConfirmationAsync(
                            booking.Owner.User.Email,
                            petOwnerName,
                            serviceName,
                            payment.Amount,
                            payment.Id
                        );
                        
                        _logger.LogInformation("Payment confirmation email sent for payment {PaymentId}", payment.Id);
                    }
                }
                catch (Exception emailEx)
                {
                    // Log email error but don't fail the payment confirmation
                    _logger.LogError(emailEx, "Failed to send payment confirmation email for payment {PaymentId}", payment.Id);
                }
            }

            return payment;
        }

        public async Task<Core.Models.Payment> ProcessRefundAsync(int paymentId, decimal amount, string reason)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
            {
                throw new InvalidOperationException("Payment not found");
            }

            if (payment.Status != PaymentStatus.Captured)
            {
                throw new InvalidOperationException("Payment cannot be refunded in its current state");
            }

            if (amount > payment.Amount)
            {
                throw new InvalidOperationException("Refund amount cannot exceed the payment amount");
            }

            // Process refund with Stripe
            var options = new RefundCreateOptions
            {
                PaymentIntent = payment.TransactionId,
                Amount = (long)(amount * 100), // Stripe uses cents
                Reason = "requested_by_customer",
                Metadata = new Dictionary<string, string>
                {
                    { "Reason", reason }
                }
            };

            var service = new RefundService();
            var refund = await service.CreateAsync(options);

            // Update payment status
            payment.Status = PaymentStatus.Refunded;
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(payment);

            // Update booking status
            var booking = await _bookingRepository.GetByIdAsync(payment.BookingId);
            if (booking != null)
            {
                booking.Status = BookingStatus.Cancelled;
                booking.Notes = booking.Notes + "\nRefund reason: " + reason;
                booking.UpdatedAt = DateTime.UtcNow;
                await _bookingRepository.UpdateAsync(booking);

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
                            amount,
                            reason
                        );
                        
                        _logger.LogInformation("Refund confirmation email sent for payment {PaymentId}", payment.Id);
                    }
                }
                catch (Exception emailEx)
                {
                    // Log email error but don't fail the refund processing
                    _logger.LogError(emailEx, "Failed to send refund confirmation email for payment {PaymentId}", payment.Id);
                }
            }

            return payment;
        }

        public async Task UpdatePaymentStatusAsync(int paymentId, PaymentStatus status)
        {
            await _paymentRepository.UpdateStatusAsync(paymentId, status);
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            return await _paymentRepository.GetTotalRevenueAsync();
        }

        public async Task<decimal> GetProviderEarningsAsync(int providerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            return await _paymentRepository.GetProviderEarningsAsync(providerId);
        }

        public async Task<decimal> CalculatePlatformFeeAsync(decimal amount)
        {
            // Platform fee is 15% of the booking amount
            return Math.Round(amount * 0.15m, 2);
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
                _logger.LogInformation("Getting payment for booking: {BookingId}", bookingId);
                
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
                _logger.LogInformation("Getting payment for transaction: {TransactionId}", transactionId);
                
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

                IEnumerable<PaymentModel> payments;

                if (query.UserId.HasValue)
                {
                    payments = await _paymentRepository.GetByUserIdAsync(query.UserId.Value).ConfigureAwait(false);
                }
                else if (query.BookingId.HasValue)
                {
                    var payment = await _paymentRepository.GetByBookingIdAsync(query.BookingId.Value).ConfigureAwait(false);
                    payments = payment != null ? new[] { payment } : Enumerable.Empty<PaymentModel>();
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
                    "createdat" => query.SortOrder == "asc" 
                        ? payments.OrderBy(p => p.CreatedAt) 
                        : payments.OrderByDescending(p => p.CreatedAt),
                    "amount" => query.SortOrder == "asc" 
                        ? payments.OrderBy(p => p.Amount) 
                        : payments.OrderByDescending(p => p.Amount),
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

                // Validate booking amount
                if (booking.TotalPrice <= 0)
                {
                    _logger.LogWarning("Invalid booking amount {Amount} for booking {BookingId}", booking.TotalPrice, request.BookingId);
                    return Result<PaymentResponse>.Failure("Invalid booking amount", "INVALID_AMOUNT");
                }

                // Calculate platform fee
                decimal platformFee = await CalculatePlatformFeeAsync(booking.TotalPrice).ConfigureAwait(false);
                decimal providerPayout = booking.TotalPrice - platformFee;

                // Create a payment intent with Stripe
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(booking.TotalPrice * 100), // Stripe uses cents
                    Currency = "cad",
                    Description = $"Payment for booking #{booking.Id}",
                    Metadata = new Dictionary<string, string>
                    {
                        { "BookingId", booking.Id.ToString() },
                        { "ServiceId", booking.ServiceId.ToString() },
                        { "OwnerId", booking.OwnerId.ToString() },
                        { "ProviderId", booking.Service?.ProviderId.ToString() ?? "0" }
                    }
                };

                var service = new PaymentIntentService();
                var intent = await service.CreateAsync(options);

                _logger.LogInformation("Stripe payment intent created: {PaymentIntentId} for booking {BookingId}", intent.Id, request.BookingId);

                // Create payment record in our database
                var payment = new PaymentModel
                {
                    BookingId = request.BookingId,
                    UserId = booking.OwnerId,
                    Amount = booking.TotalPrice,
                    PlatformFee = platformFee,
                    ProviderPayout = providerPayout,
                    TransactionId = intent.Id,
                    Status = PaymentStatus.Pending,
                    Method = Core.Models.PaymentMethod.CreditCard,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdPayment = await _paymentRepository.CreateAsync(payment).ConfigureAwait(false);
                _logger.LogInformation("Payment record created with ID {PaymentId} for booking {BookingId}", createdPayment.Id, request.BookingId);

                var response = MapToPaymentResponse(createdPayment);
                return Result<PaymentResponse>.Success(response);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating payment intent for booking {BookingId}: {StripeError}", request.BookingId, ex.Message);
                return Result<PaymentResponse>.Failure($"Payment service error: {ex.Message}", "STRIPE_ERROR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating payment intent for booking {BookingId}", request.BookingId);
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

                // Retrieve the payment intent from Stripe
                var service = new PaymentIntentService();
                var intent = await service.GetAsync(payment.TransactionId);

                // Check if the payment intent is successful
                if (intent.Status != "succeeded")
                {
                    _logger.LogWarning("Payment intent is not successful. Current status: {Status}", intent.Status);
                    return Result<PaymentResponse>.Failure(
                        $"Payment intent is not successful. Current status: {intent.Status}", 
                        "PAYMENT_NOT_SUCCEEDED");
                }

                // Update payment status
                payment.Status = PaymentStatus.Captured;
                payment.TransactionId = request.TransactionId;
                payment.UpdatedAt = DateTime.UtcNow;

                await _paymentRepository.UpdateAsync(payment).ConfigureAwait(false);

                // Update booking status
                var booking = await _bookingRepository.GetByIdAsync(payment.BookingId).ConfigureAwait(false);
                if (booking != null)
                {
                    booking.PaymentId = request.PaymentId;
                    if (booking.Status == BookingStatus.Requested)
                    {
                        booking.Status = BookingStatus.Confirmed;
                    }
                    booking.UpdatedAt = DateTime.UtcNow;
                    await _bookingRepository.UpdateAsync(booking).ConfigureAwait(false);

                    // Send payment confirmation email
                    try
                    {
                        if (booking.Owner?.User != null)
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
                }

                _logger.LogInformation("Payment confirmed successfully: {PaymentId}", request.PaymentId);

                var response = MapToPaymentResponse(payment);
                return Result<PaymentResponse>.Success(response);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error confirming payment {PaymentId}: {StripeError}", request.PaymentId, ex.Message);
                return Result<PaymentResponse>.Failure($"Payment service error: {ex.Message}", "STRIPE_ERROR");
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
                _logger.LogInformation("Processing refund for payment {PaymentId}: Amount={Amount}, Reason={Reason}", 
                    request.PaymentId, request.Amount, request.Reason);

                var payment = await _paymentRepository.GetByIdAsync(request.PaymentId).ConfigureAwait(false);
                if (payment == null)
                {
                    _logger.LogWarning("Payment not found: {PaymentId}", request.PaymentId);
                    return Result<PaymentResponse>.Failure("Payment not found", "PAYMENT_NOT_FOUND");
                }

                if (payment.Status != PaymentStatus.Captured)
                {
                    _logger.LogWarning("Payment {PaymentId} cannot be refunded in status {Status}", request.PaymentId, payment.Status);
                    return Result<PaymentResponse>.Failure(
                        "Payment cannot be refunded in its current state", 
                        "PAYMENT_NOT_REFUNDABLE");
                }

                if (request.Amount > payment.Amount)
                {
                    _logger.LogWarning("Refund amount {Amount} exceeds payment amount {PaymentAmount}", request.Amount, payment.Amount);
                    return Result<PaymentResponse>.Failure(
                        "Refund amount cannot exceed the payment amount", 
                        "INVALID_REFUND_AMOUNT");
                }

                // Process refund with Stripe
                var options = new RefundCreateOptions
                {
                    PaymentIntent = payment.TransactionId,
                    Amount = (long)(request.Amount * 100), // Stripe uses cents
                    Reason = "requested_by_customer",
                    Metadata = new Dictionary<string, string>
                    {
                        { "Reason", request.Reason }
                    }
                };

                var service = new RefundService();
                var refund = await service.CreateAsync(options);

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
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error processing refund for payment {PaymentId}: {StripeError}", request.PaymentId, ex.Message);
                return Result<PaymentResponse>.Failure($"Payment service error: {ex.Message}", "STRIPE_ERROR");
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
                _logger.LogInformation("Getting total revenue: StartDate={StartDate}, EndDate={EndDate}", startDate, endDate);

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
                _logger.LogInformation("Getting provider earnings: ProviderId={ProviderId}, StartDate={StartDate}, EndDate={EndDate}", 
                    providerId, startDate, endDate);

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
                var fee = Math.Round(amount * 0.15m, 2);
                return Result<decimal>.Success(fee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating platform fee for amount {Amount}", amount);
                return Result<decimal>.Failure("An error occurred while calculating the platform fee", "INTERNAL_ERROR");
            }
        }

        // Helper method to map Payment to PaymentResponse
        private PaymentResponse MapToPaymentResponse(PaymentModel payment)
        {
            return new PaymentResponse
            {
                Id = payment.Id,
                BookingId = payment.BookingId,
                BookingServiceName = payment.Booking?.Service?.Title ?? "Unknown Service",
                UserId = payment.UserId,
                UserName = payment.Booking?.Owner?.User != null 
                    ? $"{payment.Booking.Owner.User.FirstName} {payment.Booking.Owner.User.LastName}".Trim() 
                    : "Unknown User",
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
    }
}
