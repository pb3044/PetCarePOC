using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IUserRepository _userRepository;
        
        public NotificationService(
            INotificationRepository notificationRepository,
            IBookingRepository bookingRepository,
            IPaymentRepository paymentRepository,
            IMessageRepository messageRepository,
            IReviewRepository reviewRepository,
            IUserRepository userRepository)
        {
            _notificationRepository = notificationRepository;
            _bookingRepository = bookingRepository;
            _paymentRepository = paymentRepository;
            _messageRepository = messageRepository;
            _reviewRepository = reviewRepository;
            _userRepository = userRepository;
        }

        public async Task<Notification> GetNotificationByIdAsync(int id)
        {
            return await _notificationRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(int userId)
        {
            return await _notificationRepository.GetByUserIdAsync(userId);
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsByUserIdAsync(int userId)
        {
            return await _notificationRepository.GetUnreadByUserIdAsync(userId);
        }

        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(notification.UserId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Set default values
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;

            return await _notificationRepository.CreateAsync(notification);
        }

        public async Task MarkNotificationAsReadAsync(int notificationId)
        {
            await _notificationRepository.MarkAsReadAsync(notificationId);
        }

        public async Task MarkAllNotificationsAsReadAsync(int userId)
        {
            await _notificationRepository.MarkAllAsReadAsync(userId);
        }

        public async Task SendBookingRequestNotificationAsync(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                throw new InvalidOperationException("Booking not found");
            }

            // Create notification for service provider
            var notification = new Notification
            {
                UserId = booking.Service.ProviderId,
                Title = "New Booking Request",
                Content = $"You have a new booking request for {booking.Service.Title} on {booking.StartTime.ToShortDateString()} at {booking.StartTime.ToShortTimeString()}.",
                Type = NotificationType.BookingRequest,
                ActionUrl = $"/bookings/{bookingId}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.CreateAsync(notification);
        }

        public async Task SendBookingConfirmationNotificationAsync(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                throw new InvalidOperationException("Booking not found");
            }

            // Create notification for pet owner
            var notification = new Notification
            {
                UserId = booking.OwnerId,
                Title = "Booking Confirmed",
                Content = $"Your booking for {booking.Service.Title} on {booking.StartTime.ToShortDateString()} at {booking.StartTime.ToShortTimeString()} has been confirmed.",
                Type = NotificationType.BookingConfirmation,
                ActionUrl = $"/bookings/{bookingId}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.CreateAsync(notification);
        }

        public async Task SendBookingCancellationNotificationAsync(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
            {
                throw new InvalidOperationException("Booking not found");
            }

            // Create notification for pet owner or service provider (depending on who cancelled)
            // For simplicity, we'll notify both parties
            
            // Notify pet owner
            var ownerNotification = new Notification
            {
                UserId = booking.OwnerId,
                Title = "Booking Cancelled",
                Content = $"Your booking for {booking.Service.Title} on {booking.StartTime.ToShortDateString()} at {booking.StartTime.ToShortTimeString()} has been cancelled.",
                Type = NotificationType.BookingCancellation,
                ActionUrl = $"/bookings/{bookingId}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.CreateAsync(ownerNotification);

            // Notify service provider
            var providerNotification = new Notification
            {
                UserId = booking.Service.ProviderId,
                Title = "Booking Cancelled",
                Content = $"A booking for {booking.Service.Title} on {booking.StartTime.ToShortDateString()} at {booking.StartTime.ToShortTimeString()} has been cancelled.",
                Type = NotificationType.BookingCancellation,
                ActionUrl = $"/bookings/{bookingId}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.CreateAsync(providerNotification);
        }

        public async Task SendPaymentConfirmationNotificationAsync(int paymentId)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
            {
                throw new InvalidOperationException("Payment not found");
            }

            var booking = await _bookingRepository.GetByIdAsync(payment.BookingId);
            if (booking == null)
            {
                throw new InvalidOperationException("Booking not found");
            }

            // Create notification for pet owner
            var ownerNotification = new Notification
            {
                UserId = payment.UserId,
                Title = "Payment Confirmed",
                Content = $"Your payment of ${payment.Amount} for {booking.Service.Title} has been confirmed.",
                Type = NotificationType.PaymentConfirmation,
                ActionUrl = $"/bookings/{booking.Id}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.CreateAsync(ownerNotification);

            // Create notification for service provider
            var providerNotification = new Notification
            {
                UserId = booking.Service.ProviderId,
                Title = "Payment Received",
                Content = $"You have received a payment of ${payment.ProviderPayout} for {booking.Service.Title}.",
                Type = NotificationType.PaymentConfirmation,
                ActionUrl = $"/bookings/{booking.Id}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.CreateAsync(providerNotification);
        }

        public async Task SendNewMessageNotificationAsync(int messageId)
        {
            var message = await _messageRepository.GetByIdAsync(messageId);
            if (message == null)
            {
                throw new InvalidOperationException("Message not found");
            }

            var sender = await _userRepository.GetByIdAsync(message.SenderId);
            if (sender == null)
            {
                throw new InvalidOperationException("Sender not found");
            }

            // Create notification for receiver
            var notification = new Notification
            {
                UserId = message.ReceiverId,
                Title = "New Message",
                Content = $"You have a new message from {sender.FirstName} {sender.LastName}.",
                Type = NotificationType.NewMessage,
                ActionUrl = $"/messages/{message.SenderId}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.CreateAsync(notification);
        }

        public async Task SendNewReviewNotificationAsync(int reviewId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
            {
                throw new InvalidOperationException("Review not found");
            }

            var reviewer = await _userRepository.GetByIdAsync(review.ReviewerId);
            if (reviewer == null)
            {
                throw new InvalidOperationException("Reviewer not found");
            }

            // Create notification for reviewee
            var notification = new Notification
            {
                UserId = review.RevieweeId,
                Title = "New Review",
                Content = $"You have received a {review.Rating}-star review from {reviewer.FirstName} {reviewer.LastName}.",
                Type = NotificationType.NewReview,
                ActionUrl = $"/reviews/{reviewId}",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.CreateAsync(notification);
        }
    }
}
