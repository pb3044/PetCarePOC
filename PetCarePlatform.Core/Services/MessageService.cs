using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;

        public MessageService(IMessageRepository messageRepository, IUserRepository userRepository)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
        }

        public async Task<Message> GetMessageByIdAsync(int id)
        {
            return await _messageRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Message>> GetConversationAsync(int user1Id, int user2Id)
        {
            return await _messageRepository.GetConversationAsync(user1Id, user2Id);
        }

        public async Task<IEnumerable<Message>> GetUnreadMessagesAsync(int userId)
        {
            return await _messageRepository.GetUnreadMessagesAsync(userId);
        }

        public async Task<IEnumerable<Message>> GetMessagesByBookingIdAsync(int bookingId)
        {
            return await _messageRepository.GetByBookingIdAsync(bookingId);
        }

        public async Task<Message> SendMessageAsync(Message message)
        {
            // Validate sender and receiver exist
            var sender = await _userRepository.GetByIdAsync(message.SenderId);
            if (sender == null)
            {
                throw new InvalidOperationException("Sender not found");
            }

            var receiver = await _userRepository.GetByIdAsync(message.ReceiverId);
            if (receiver == null)
            {
                throw new InvalidOperationException("Receiver not found");
            }

            // Set default values
            message.CreatedAt = DateTime.UtcNow;
            message.IsRead = false;
            message.ReadAt = null;

            return await _messageRepository.CreateAsync(message);
        }

        public async Task MarkMessageAsReadAsync(int messageId)
        {
            await _messageRepository.MarkAsReadAsync(messageId);
        }

        public async Task MarkAllMessagesAsReadAsync(int receiverId)
        {
            await _messageRepository.MarkAllAsReadAsync(receiverId);
        }

        public async Task<IEnumerable<ApplicationUser>> GetConversationPartnersAsync(int userId)
        {
            // Get all messages sent by or received by the user
            var sentMessages = await _messageRepository.GetBySenderIdAsync(userId);
            var receivedMessages = await _messageRepository.GetByReceiverIdAsync(userId);

            // Extract unique user IDs
            var partnerIds = new HashSet<int>();

            foreach (var message in sentMessages)
            {
                partnerIds.Add(message.ReceiverId);
            }

            foreach (var message in receivedMessages)
            {
                partnerIds.Add(message.SenderId);
            }

            // Remove the user's own ID if it's in the set
            partnerIds.Remove(userId);

            // Get user objects for each partner ID
            var partners = new List<ApplicationUser>();
            foreach (var partnerId in partnerIds)
            {
                var partner = await _userRepository.GetByIdAsync(partnerId);
                if (partner != null)
                {
                    partners.Add(partner);
                }
            }

            return partners;
        }
    }
}
