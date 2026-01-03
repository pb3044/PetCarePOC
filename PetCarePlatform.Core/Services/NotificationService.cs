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
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<NotificationService> _logger;
        
        public NotificationService(
            INotificationRepository notificationRepository,
            IBookingRepository bookingRepository,
            IPaymentRepository paymentRepository,
            IMessageRepository messageRepository,
            IReviewRepository reviewRepository,
            IUserRepository userRepository,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        // ============================================
        // Enterprise Pattern Methods (Result-based)
        // ============================================

        public async Task<Result<NotificationResponse>> GetNotificationByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting notification by ID: {NotificationId}", id);
                
                var notification = await _notificationRepository.GetByIdAsync(id).ConfigureAwait(false);
                if (notification == null)
                {
                    _logger.LogWarning("Notification not found: {NotificationId}", id);
                    return Result<NotificationResponse>.Failure("Notification not found", "NOTIFICATION_NOT_FOUND");
                }

                var response = MapToNotificationResponse(notification);
                return Result<NotificationResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notification {NotificationId}", id);
                return Result<NotificationResponse>.Failure("An error occurred while retrieving the notification", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<PagedResult<NotificationResponse>>> GetNotificationsAsync(NotificationQuery query, int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting notifications for user {UserId} with query", userId);

                var notifications = await _notificationRepository.GetByUserIdAsync(userId).ConfigureAwait(false);

                // Apply filters
                if (query.Type.HasValue)
                {
                    notifications = notifications.Where(n => n.Type == query.Type.Value);
                }

                if (query.IsRead.HasValue)
                {
                    notifications = notifications.Where(n => n.IsRead == query.IsRead.Value);
                }

                if (query.FromDate.HasValue)
                {
                    notifications = notifications.Where(n => n.CreatedAt >= query.FromDate.Value);
                }

                if (query.ToDate.HasValue)
                {
                    notifications = notifications.Where(n => n.CreatedAt <= query.ToDate.Value);
                }

                if (!string.IsNullOrWhiteSpace(query.SearchTerm))
                {
                    var searchTerm = query.SearchTerm.ToLower();
                    notifications = notifications.Where(n => 
                        n.Title.ToLower().Contains(searchTerm) || 
                        n.Content.ToLower().Contains(searchTerm));
                }

                // Apply sorting
                notifications = query.SortBy?.ToLower() switch
                {
                    "createdat" => query.SortOrder == "asc" 
                        ? notifications.OrderBy(n => n.CreatedAt) 
                        : notifications.OrderByDescending(n => n.CreatedAt),
                    "isread" => query.SortOrder == "asc" 
                        ? notifications.OrderBy(n => n.IsRead) 
                        : notifications.OrderByDescending(n => n.IsRead),
                    "type" => query.SortOrder == "asc" 
                        ? notifications.OrderBy(n => n.Type) 
                        : notifications.OrderByDescending(n => n.Type),
                    _ => notifications.OrderByDescending(n => n.CreatedAt)
                };

                var totalCount = notifications.Count();
                var items = notifications
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(MapToNotificationResponse)
                    .ToList();

                var pagedResult = new PagedResult<NotificationResponse>(
                    items,
                    totalCount,
                    query.PageNumber,
                    query.PageSize
                );

                return Result<PagedResult<NotificationResponse>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                return Result<PagedResult<NotificationResponse>>.Failure("An error occurred while retrieving notifications", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<NotificationResponse>> CreateNotificationAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating notification for user {UserId}", request.UserId);

                // Validate user exists
                var user = await _userRepository.GetByIdAsync(request.UserId).ConfigureAwait(false);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", request.UserId);
                    return Result<NotificationResponse>.Failure("User not found", "USER_NOT_FOUND");
                }

                // Create notification
                var notification = new Notification
                {
                    UserId = request.UserId,
                    Title = request.Title,
                    Content = request.Content,
                    Type = request.Type,
                    ActionUrl = request.ActionUrl,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    ReadAt = null
                };

                var createdNotification = await _notificationRepository.CreateAsync(notification).ConfigureAwait(false);

                _logger.LogInformation("Notification created successfully: {NotificationId}", createdNotification.Id);

                var response = MapToNotificationResponse(createdNotification);
                return Result<NotificationResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for user {UserId}", request.UserId);
                return Result<NotificationResponse>.Failure("An error occurred while creating the notification", "INTERNAL_ERROR");
            }
        }

        public async Task<Result> MarkNotificationAsReadAsync(int notificationId, int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Marking notification {NotificationId} as read by user {UserId}", notificationId, userId);

                var notification = await _notificationRepository.GetByIdAsync(notificationId).ConfigureAwait(false);
                if (notification == null)
                {
                    _logger.LogWarning("Notification not found: {NotificationId}", notificationId);
                    return Result.Failure("Notification not found", "NOTIFICATION_NOT_FOUND");
                }

                // Validate user owns the notification
                if (notification.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} does not own notification {NotificationId}", userId, notificationId);
                    return Result.Failure("You can only mark your own notifications as read", "UNAUTHORIZED");
                }

                if (!notification.IsRead)
                {
                    await _notificationRepository.MarkAsReadAsync(notificationId).ConfigureAwait(false);
                    _logger.LogInformation("Notification {NotificationId} marked as read", notificationId);
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
                return Result.Failure("An error occurred while marking the notification as read", "INTERNAL_ERROR");
            }
        }

        public async Task<Result> MarkAllNotificationsAsReadAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Marking all notifications as read for user {UserId}", userId);

                await _notificationRepository.MarkAllAsReadAsync(userId).ConfigureAwait(false);
                _logger.LogInformation("All notifications marked as read for user {UserId}", userId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
                return Result.Failure("An error occurred while marking notifications as read", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<int>> GetUnreadNotificationCountAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting unread notification count for user {UserId}", userId);

                var unreadNotifications = await _notificationRepository.GetUnreadByUserIdAsync(userId).ConfigureAwait(false);
                var count = unreadNotifications.Count();

                return Result<int>.Success(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notification count for user {UserId}", userId);
                return Result<int>.Failure("An error occurred while retrieving unread notification count", "INTERNAL_ERROR");
            }
        }

        // Helper method to map Notification to NotificationResponse
        private NotificationResponse MapToNotificationResponse(Notification notification)
        {
            return new NotificationResponse
            {
                Id = notification.Id,
                UserId = notification.UserId,
                UserName = $"{notification.User?.FirstName} {notification.User?.LastName}".Trim(),
                Title = notification.Title,
                Content = notification.Content,
                Type = notification.Type,
                ActionUrl = notification.ActionUrl,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt
            };
        }
    }
}
