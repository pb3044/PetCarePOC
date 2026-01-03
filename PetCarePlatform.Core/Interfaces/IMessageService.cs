using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PetCarePlatform.Core.Common;
using PetCarePlatform.Core.DTOs.Queries;
using PetCarePlatform.Core.DTOs.Requests;
using PetCarePlatform.Core.DTOs.Responses;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IMessageService
    {
        Task<Result<MessageResponse>> GetMessageByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<PagedResult<MessageResponse>>> GetMessagesAsync(MessageQuery query, int currentUserId, CancellationToken cancellationToken = default);
        Task<Result<PagedResult<MessageResponse>>> GetConversationAsync(int user1Id, int user2Id, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
        Task<Result<MessageResponse>> SendMessageAsync(SendMessageRequest request, int senderId, CancellationToken cancellationToken = default);
        Task<Result> MarkMessageAsReadAsync(int messageId, int userId, CancellationToken cancellationToken = default);
        Task<Result> MarkAllMessagesAsReadAsync(int receiverId, int userId, CancellationToken cancellationToken = default);
        Task<Result<List<ConversationSummaryResponse>>> GetConversationSummariesAsync(int userId, CancellationToken cancellationToken = default);
        Task<Result<int>> GetUnreadMessageCountAsync(int userId, CancellationToken cancellationToken = default);
    }
}
