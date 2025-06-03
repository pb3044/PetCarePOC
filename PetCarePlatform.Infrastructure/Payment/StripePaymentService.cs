using Microsoft.Extensions.Configuration;
using PetCarePlatform.Core.Interfaces;
using Stripe;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Infrastructure.Payment
{
    public class StripePaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentRepository _paymentRepository;

        public StripePaymentService(
            IConfiguration configuration,
            IBookingRepository bookingRepository,
            IPaymentRepository paymentRepository)
        {
            _configuration = configuration;
            _apiKey = _configuration["Stripe:SecretKey"];
            _bookingRepository = bookingRepository;
            _paymentRepository = paymentRepository;
            
            // Initialize Stripe
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

        public async Task<IEnumerable<Core.Models.Payment>> GetPaymentsByUserIdAsync(int userId)
        {
            return await _paymentRepository.GetByUserIdAsync(userId);
        }

        public async Task<Core.Models.Payment> CreatePaymentIntentAsync(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                throw new InvalidOperationException("Booking not found");
            }

            // Check if payment already exists
            var existingPayment = await _paymentRepository.GetByBookingIdAsync(bookingId);
            if (existingPayment != null)
            {
                throw new InvalidOperationException("Payment already exists for this booking");
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

            return await _paymentRepository.CreateAsync(payment);
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
    }
}
