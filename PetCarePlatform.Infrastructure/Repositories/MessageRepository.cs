using Microsoft.EntityFrameworkCore;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Infrastructure.Data;

namespace PetCarePlatform.Infrastructure.Repositories
{
    public class MessageRepository : BaseRepository<Message>, IMessageRepository
    {
        public MessageRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<Message?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public override async Task<IEnumerable<Message>> GetAllAsync()
        {
            return await _dbSet
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetBySenderIdAsync(int senderId)
        {
            return await _dbSet
                .Where(m => m.SenderId == senderId)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetByReceiverIdAsync(int receiverId)
        {
            return await _dbSet
                .Where(m => m.ReceiverId == receiverId)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetByBookingIdAsync(int bookingId)
        {
            return await _dbSet
                .Where(m => m.BookingId == bookingId)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetConversationAsync(int user1Id, int user2Id)
        {
            return await _dbSet
                .Where(m => (m.SenderId == user1Id && m.ReceiverId == user2Id) ||
                           (m.SenderId == user2Id && m.ReceiverId == user1Id))
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetUnreadMessagesAsync(int userId)
        {
            return await _dbSet
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(Message message)
        {
            _dbSet.Update(message);
            await _context.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(int messageId)
        {
            var message = await GetByIdAsync(messageId);
            if (message != null && !message.IsRead)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(int receiverId)
        {
            var unreadMessages = await _dbSet
                .Where(m => m.ReceiverId == receiverId && !m.IsRead)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}

