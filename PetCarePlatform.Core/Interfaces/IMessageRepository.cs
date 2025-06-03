using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IMessageRepository
    {
        Task<Message> GetByIdAsync(int id);
        Task<IEnumerable<Message>> GetAllAsync();
        Task<IEnumerable<Message>> GetBySenderIdAsync(int senderId);
        Task<IEnumerable<Message>> GetByReceiverIdAsync(int receiverId);
        Task<IEnumerable<Message>> GetByBookingIdAsync(int bookingId);
        Task<IEnumerable<Message>> GetConversationAsync(int user1Id, int user2Id);
        Task<IEnumerable<Message>> GetUnreadMessagesAsync(int userId);
        Task<Message> CreateAsync(Message message);
        Task UpdateAsync(Message message);
        Task MarkAsReadAsync(int messageId);
        Task MarkAllAsReadAsync(int receiverId);
        Task DeleteAsync(int id);
    }
}
