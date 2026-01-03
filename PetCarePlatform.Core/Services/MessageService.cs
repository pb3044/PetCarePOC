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
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<MessageService> _logger;

        public MessageService(
            IMessageRepository messageRepository, 
            IUserRepository userRepository,
            IBookingRepository bookingRepository,
            ILogger<MessageService> logger)
        {
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        // ============================================
        // Enterprise Pattern Methods (Result-based)
        // ============================================

        public async Task<Result<MessageResponse>> GetMessageByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting message by ID: {MessageId}", id);
                
                var message = await _messageRepository.GetByIdAsync(id);
                if (message == null)
                {
                    _logger.LogWarning("Message not found: {MessageId}", id);
                    return Result<MessageResponse>.Failure("Message not found", "MESSAGE_NOT_FOUND");
                }

                var response = MapToMessageResponse(message);
                return Result<MessageResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting message {MessageId}", id);
                return Result<MessageResponse>.Failure("An error occurred while retrieving the message", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<PagedResult<MessageResponse>>> GetMessagesAsync(MessageQuery query, int currentUserId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting messages with query for user {UserId}", currentUserId);

                IEnumerable<Message> messages;

                // Get messages based on query filters
                if (query.ReceiverId.HasValue)
                {
                    // Messages where current user is sender and query receiver is receiver
                    if (query.ReceiverId.Value == currentUserId)
                    {
                        messages = await _messageRepository.GetByReceiverIdAsync(currentUserId);
                    }
                    else
                    {
                        // Get conversation between current user and receiver
                        messages = await _messageRepository.GetConversationAsync(currentUserId, query.ReceiverId.Value);
                    }
                }
                else if (query.BookingId.HasValue)
                {
                    messages = await _messageRepository.GetByBookingIdAsync(query.BookingId.Value);
                }
                else
                {
                    // Get all messages where current user is sender or receiver
                    var sentMessages = await _messageRepository.GetBySenderIdAsync(currentUserId);
                    var receivedMessages = await _messageRepository.GetByReceiverIdAsync(currentUserId);
                    messages = sentMessages.Concat(receivedMessages).Distinct();
                }

                // Apply filters
                if (query.IsRead.HasValue)
                {
                    messages = messages.Where(m => m.IsRead == query.IsRead.Value);
                }

                if (query.FromDate.HasValue)
                {
                    messages = messages.Where(m => m.CreatedAt >= query.FromDate.Value);
                }

                if (query.ToDate.HasValue)
                {
                    messages = messages.Where(m => m.CreatedAt <= query.ToDate.Value);
                }

                if (!string.IsNullOrEmpty(query.SearchTerm))
                {
                    var searchTerm = query.SearchTerm.ToLower();
                    messages = messages.Where(m => m.Content.ToLower().Contains(searchTerm));
                }

                // Apply sorting
                messages = query.SortBy?.ToLower() switch
                {
                    "createdat" => query.SortOrder == "asc" 
                        ? messages.OrderBy(m => m.CreatedAt) 
                        : messages.OrderByDescending(m => m.CreatedAt),
                    "isread" => query.SortOrder == "asc" 
                        ? messages.OrderBy(m => m.IsRead) 
                        : messages.OrderByDescending(m => m.IsRead),
                    _ => messages.OrderByDescending(m => m.CreatedAt)
                };

                var totalCount = messages.Count();
                var items = messages
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(MapToMessageResponse)
                    .ToList();

                var pagedResult = new PagedResult<MessageResponse>(
                    items,
                    totalCount,
                    query.PageNumber,
                    query.PageSize
                );

                return Result<PagedResult<MessageResponse>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for user {UserId}", currentUserId);
                return Result<PagedResult<MessageResponse>>.Failure("An error occurred while retrieving messages", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<PagedResult<MessageResponse>>> GetConversationAsync(int user1Id, int user2Id, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting conversation between user {User1Id} and {User2Id}", user1Id, user2Id);

                var messages = await _messageRepository.GetConversationAsync(user1Id, user2Id);
                
                var orderedMessages = messages.OrderBy(m => m.CreatedAt);
                var totalCount = orderedMessages.Count();
                var items = orderedMessages
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(MapToMessageResponse)
                    .ToList();

                var pagedResult = new PagedResult<MessageResponse>(
                    items,
                    totalCount,
                    pageNumber,
                    pageSize
                );

                return Result<PagedResult<MessageResponse>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation between user {User1Id} and {User2Id}", user1Id, user2Id);
                return Result<PagedResult<MessageResponse>>.Failure("An error occurred while retrieving the conversation", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<MessageResponse>> SendMessageAsync(SendMessageRequest request, int senderId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Sending message from user {SenderId} to user {ReceiverId}", senderId, request.ReceiverId);

                // Validate sender exists
                var sender = await _userRepository.GetByIdAsync(senderId);
                if (sender == null)
                {
                    _logger.LogWarning("Sender not found: {SenderId}", senderId);
                    return Result<MessageResponse>.Failure("Sender not found", "SENDER_NOT_FOUND");
                }

                // Validate receiver exists
                var receiver = await _userRepository.GetByIdAsync(request.ReceiverId);
                if (receiver == null)
                {
                    _logger.LogWarning("Receiver not found: {ReceiverId}", request.ReceiverId);
                    return Result<MessageResponse>.Failure("Receiver not found", "RECEIVER_NOT_FOUND");
                }

                // Validate booking exists if specified
                if (request.BookingId.HasValue)
                {
                    var booking = await _bookingRepository.GetByIdAsync(request.BookingId.Value);
                    if (booking == null)
                    {
                        _logger.LogWarning("Booking not found: {BookingId}", request.BookingId.Value);
                        return Result<MessageResponse>.Failure("Booking not found", "BOOKING_NOT_FOUND");
                    }
                }

                // Create message
                var message = new Message
                {
                    SenderId = senderId,
                    ReceiverId = request.ReceiverId,
                    BookingId = request.BookingId,
                    Content = request.Content ?? string.Empty,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    ReadAt = null
                };

                var createdMessage = await _messageRepository.CreateAsync(message);

                _logger.LogInformation("Message sent successfully: {MessageId}", createdMessage.Id);

                var response = MapToMessageResponse(createdMessage);
                return Result<MessageResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message from user {SenderId}", senderId);
                return Result<MessageResponse>.Failure("An error occurred while sending the message", "INTERNAL_ERROR");
            }
        }

        public async Task<Result> MarkMessageAsReadAsync(int messageId, int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Marking message {MessageId} as read by user {UserId}", messageId, userId);

                var message = await _messageRepository.GetByIdAsync(messageId);
                if (message == null)
                {
                    _logger.LogWarning("Message not found: {MessageId}", messageId);
                    return Result.Failure("Message not found", "MESSAGE_NOT_FOUND");
                }

                // Verify that the user is the receiver
                if (message.ReceiverId != userId)
                {
                    _logger.LogWarning("User {UserId} is not the receiver of message {MessageId}", userId, messageId);
                    return Result.Failure("You are not authorized to mark this message as read", "UNAUTHORIZED");
                }

                await _messageRepository.MarkAsReadAsync(messageId);

                _logger.LogInformation("Message {MessageId} marked as read successfully", messageId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message {MessageId} as read", messageId);
                return Result.Failure("An error occurred while marking the message as read", "INTERNAL_ERROR");
            }
        }

        public async Task<Result> MarkAllMessagesAsReadAsync(int receiverId, int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Marking all messages as read for receiver {ReceiverId} by user {UserId}", receiverId, userId);

                // Verify that the user is marking their own messages as read
                if (receiverId != userId)
                {
                    _logger.LogWarning("User {UserId} is not authorized to mark messages for receiver {ReceiverId}", userId, receiverId);
                    return Result.Failure("You are not authorized to mark these messages as read", "UNAUTHORIZED");
                }

                await _messageRepository.MarkAllAsReadAsync(receiverId);

                _logger.LogInformation("All messages marked as read successfully for receiver {ReceiverId}", receiverId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all messages as read for receiver {ReceiverId}", receiverId);
                return Result.Failure("An error occurred while marking messages as read", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<List<ConversationSummaryResponse>>> GetConversationSummariesAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting conversation summaries for user {UserId}", userId);

                // Get all messages where user is sender or receiver
                var sentMessages = await _messageRepository.GetBySenderIdAsync(userId);
                var receivedMessages = await _messageRepository.GetByReceiverIdAsync(userId);

                // Group messages by conversation partner
                var conversationPartners = new Dictionary<int, ConversationSummaryResponse>();

                // Process sent messages
                foreach (var message in sentMessages)
                {
                    var partnerId = message.ReceiverId;
                    if (!conversationPartners.ContainsKey(partnerId))
                    {
                        var partner = await _userRepository.GetByIdAsync(partnerId);
                        conversationPartners[partnerId] = new ConversationSummaryResponse
                        {
                            PartnerId = partnerId,
                            PartnerName = partner != null ? $"{partner.FirstName} {partner.LastName}".Trim() : "Unknown User",
                            PartnerPhotoUrl = null, // Photo URL not available in current model
                            LastMessagePreview = string.Empty,
                            LastMessageDate = null,
                            UnreadCount = 0,
                            IsRead = true
                        };
                    }

                    var summary = conversationPartners[partnerId];
                    if (message.CreatedAt > (summary.LastMessageDate ?? DateTime.MinValue))
                    {
                        summary.LastMessageDate = message.CreatedAt;
                        summary.LastMessagePreview = message.Content.Length > 50 
                            ? message.Content.Substring(0, 50) + "..." 
                            : message.Content;
                        summary.IsRead = true; // Sent messages are always read
                    }
                }

                // Process received messages
                foreach (var message in receivedMessages)
                {
                    var partnerId = message.SenderId;
                    if (!conversationPartners.ContainsKey(partnerId))
                    {
                        var partner = await _userRepository.GetByIdAsync(partnerId);
                        conversationPartners[partnerId] = new ConversationSummaryResponse
                        {
                            PartnerId = partnerId,
                            PartnerName = partner != null ? $"{partner.FirstName} {partner.LastName}".Trim() : "Unknown User",
                            PartnerPhotoUrl = null, // Photo URL not available in current model
                            LastMessagePreview = string.Empty,
                            LastMessageDate = null,
                            UnreadCount = 0,
                            IsRead = true
                        };
                    }

                    var summary = conversationPartners[partnerId];
                    if (message.CreatedAt > (summary.LastMessageDate ?? DateTime.MinValue))
                    {
                        summary.LastMessageDate = message.CreatedAt;
                        summary.LastMessagePreview = message.Content.Length > 50 
                            ? message.Content.Substring(0, 50) + "..." 
                            : message.Content;
                        summary.IsRead = message.IsRead;
                    }

                    if (!message.IsRead)
                    {
                        summary.UnreadCount++;
                    }
                }

                var summaries = conversationPartners.Values
                    .OrderByDescending(s => s.LastMessageDate ?? DateTime.MinValue)
                    .ToList();

                return Result<List<ConversationSummaryResponse>>.Success(summaries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation summaries for user {UserId}", userId);
                return Result<List<ConversationSummaryResponse>>.Failure("An error occurred while retrieving conversation summaries", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<int>> GetUnreadMessageCountAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting unread message count for user {UserId}", userId);

                var unreadMessages = await _messageRepository.GetUnreadMessagesAsync(userId);
                var count = unreadMessages.Count();

                return Result<int>.Success(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread message count for user {UserId}", userId);
                return Result<int>.Failure("An error occurred while retrieving unread message count", "INTERNAL_ERROR");
            }
        }

        // Helper method to map Message to MessageResponse
        private MessageResponse MapToMessageResponse(Message message)
        {
            return new MessageResponse
            {
                Id = message.Id,
                SenderId = message.SenderId,
                SenderName = message.Sender != null 
                    ? $"{message.Sender.FirstName} {message.Sender.LastName}".Trim() 
                    : "Unknown User",
                SenderPhotoUrl = null, // Photo URL not available in current model
                ReceiverId = message.ReceiverId,
                ReceiverName = message.Receiver != null 
                    ? $"{message.Receiver.FirstName} {message.Receiver.LastName}".Trim() 
                    : "Unknown User",
                ReceiverPhotoUrl = null, // Photo URL not available in current model
                BookingId = message.BookingId,
                BookingTitle = message.Booking?.Service?.Title ?? null,
                Content = message.Content ?? string.Empty,
                IsRead = message.IsRead,
                CreatedAt = message.CreatedAt,
                ReadAt = message.ReadAt,
                AttachmentUrls = new List<string>() // Not supported in current model
            };
        }
    }
}
