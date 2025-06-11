using Microsoft.EntityFrameworkCore;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Infrastructure.Data;

namespace PetCarePlatform.Infrastructure.Repositories
{
    public class NotificationRepository : BaseRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<Notification?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public override async Task<IEnumerable<Notification>> GetAllAsync()
        {
            return await _dbSet
                .Include(n => n.User)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetByTypeAsync(NotificationType type)
        {
            return await _dbSet
                .Where(n => n.Type == type)
                .Include(n => n.User)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(Notification notification)
        {
            _dbSet.Update(notification);
            await _context.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await GetByIdAsync(notificationId);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var unreadNotifications = await _dbSet
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}

