using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IMessageService
    {
        Task<Message> GetMessageByIdAsync(int id);
        Task<IEnumerable<Message>> GetConversationAsync(int user1Id, int user2Id);
        Task<IEnumerable<Message>> GetUnreadMessagesAsync(int userId);
        Task<IEnumerable<Message>> GetMessagesByBookingIdAsync(int bookingId);
        Task<Message> SendMessageAsync(Message message);
        Task MarkMessageAsReadAsync(int messageId);
        Task MarkAllMessagesAsReadAsync(int receiverId);
        Task<IEnumerable<ApplicationUser>> GetConversationPartnersAsync(int userId);
    }
}
