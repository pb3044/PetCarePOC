using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface INotificationService
    {
        Task<Notification> GetNotificationByIdAsync(int id);
        Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(int userId);
        Task<IEnumerable<Notification>> GetUnreadNotificationsByUserIdAsync(int userId);
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task MarkNotificationAsReadAsync(int notificationId);
        Task MarkAllNotificationsAsReadAsync(int userId);
        Task SendBookingRequestNotificationAsync(int bookingId);
        Task SendBookingConfirmationNotificationAsync(int bookingId);
        Task SendBookingCancellationNotificationAsync(int bookingId);
        Task SendPaymentConfirmationNotificationAsync(int paymentId);
        Task SendNewMessageNotificationAsync(int messageId);
        Task SendNewReviewNotificationAsync(int reviewId);
    }
}
