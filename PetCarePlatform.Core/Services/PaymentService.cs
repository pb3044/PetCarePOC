using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IBookingRepository _bookingRepository;
        
        public PaymentService(IPaymentRepository paymentRepository, IBookingRepository bookingRepository)
        {
            _paymentRepository = paymentRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<Payment> GetPaymentByIdAsync(int id)
        {
            return await _paymentRepository.GetByIdAsync(id);
        }

        public async Task<Payment> GetPaymentByBookingIdAsync(int bookingId)
        {
            return await _paymentRepository.GetByBookingIdAsync(bookingId);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(int userId)
        {
            return await _paymentRepository.GetByUserIdAsync(userId);
        }

        public async Task<Payment> CreatePaymentIntentAsync(int bookingId)
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

            // Create a new payment
            var payment = new Payment
            {
                BookingId = bookingId,
                UserId = booking.OwnerId,
                Amount = booking.TotalPrice,
                PlatformFee = await CalculatePlatformFeeAsync(booking.TotalPrice),
                ProviderPayout = booking.TotalPrice - await CalculatePlatformFeeAsync(booking.TotalPrice),
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return await _paymentRepository.CreateAsync(payment);
        }

        public async Task<Payment> ConfirmPaymentAsync(int paymentId, string transactionId)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
            {
                throw new InvalidOperationException("Payment not found");
            }

            // Update payment details
            payment.TransactionId = transactionId;
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

        public async Task<Payment> ProcessRefundAsync(int paymentId, decimal amount, string reason)
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
