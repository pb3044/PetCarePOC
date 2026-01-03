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
    public interface INotificationService
    {
        Task<Result<NotificationResponse>> GetNotificationByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<PagedResult<NotificationResponse>>> GetNotificationsAsync(NotificationQuery query, int userId, CancellationToken cancellationToken = default);
        Task<Result<NotificationResponse>> CreateNotificationAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default);
        Task<Result> MarkNotificationAsReadAsync(int notificationId, int userId, CancellationToken cancellationToken = default);
        Task<Result> MarkAllNotificationsAsReadAsync(int userId, CancellationToken cancellationToken = default);
        Task<Result<int>> GetUnreadNotificationCountAsync(int userId, CancellationToken cancellationToken = default);
    }
}
