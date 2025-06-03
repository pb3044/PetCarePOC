using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification> GetByIdAsync(int id);
        Task<IEnumerable<Notification>> GetAllAsync();
        Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(int userId);
        Task<IEnumerable<Notification>> GetByTypeAsync(NotificationType type);
        Task<Notification> CreateAsync(Notification notification);
        Task UpdateAsync(Notification notification);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(int userId);
        Task DeleteAsync(int id);
    }
}
