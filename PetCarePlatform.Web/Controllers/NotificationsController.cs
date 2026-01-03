using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading;
using System.Threading.Tasks;
using PetCarePlatform.Core.DTOs.Queries;
using PetCarePlatform.Core.Interfaces;
using System.Security.Claims;

namespace PetCarePlatform.Web.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string? type = null, bool? isRead = null)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Get unread count
            var unreadCountResult = await _notificationService.GetUnreadNotificationCountAsync(userId);
            ViewBag.UnreadCount = unreadCountResult.IsSuccess ? unreadCountResult.Value : 0;

            // Get notifications with query
            var query = new NotificationQuery
            {
                PageNumber = page,
                PageSize = pageSize,
                Type = !string.IsNullOrEmpty(type) && Enum.TryParse<PetCarePlatform.Core.Models.NotificationType>(type, out var notificationType) 
                    ? notificationType 
                    : null,
                IsRead = isRead,
                SortBy = "CreatedAt",
                SortOrder = "desc"
            };

            var result = await _notificationService.GetNotificationsAsync(query, userId);
            
            if (result.IsFailure)
            {
                TempData["Error"] = result.ErrorMessage;
                return View(new PetCarePlatform.Core.Common.PagedResult<PetCarePlatform.Core.DTOs.Responses.NotificationResponse>(
                    new List<PetCarePlatform.Core.DTOs.Responses.NotificationResponse>(),
                    0,
                    1,
                    20
                ));
            }

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = result.Value!.TotalPages;
            ViewBag.TotalCount = result.Value.TotalCount;

            return View(result.Value);
        }

        public async Task<IActionResult> Unread(int page = 1, int pageSize = 20)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var query = new NotificationQuery
            {
                PageNumber = page,
                PageSize = pageSize,
                IsRead = false,
                SortBy = "CreatedAt",
                SortOrder = "desc"
            };

            var result = await _notificationService.GetNotificationsAsync(query, userId);
            
            if (result.IsFailure)
            {
                TempData["Error"] = result.ErrorMessage;
                return View(new PetCarePlatform.Core.Common.PagedResult<PetCarePlatform.Core.DTOs.Responses.NotificationResponse>(
                    new List<PetCarePlatform.Core.DTOs.Responses.NotificationResponse>(),
                    0,
                    1,
                    20
                ));
            }

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = result.Value!.TotalPages;
            ViewBag.TotalCount = result.Value.TotalCount;

            return View(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var result = await _notificationService.MarkNotificationAsReadAsync(id, userId);
            
            if (result.IsFailure)
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }

            return Json(new { success = true, message = "Notification marked as read" });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var result = await _notificationService.MarkAllNotificationsAsReadAsync(userId, CancellationToken.None);
            
            if (result.IsFailure)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }
            else
            {
                TempData["SuccessMessage"] = "All notifications marked as read!";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var result = await _notificationService.GetUnreadNotificationCountAsync(userId, CancellationToken.None);
            
            if (result.IsFailure)
            {
                return Json(new { success = false, count = 0 });
            }

            return Json(new { success = true, count = result.Value });
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var result = await _notificationService.GetNotificationByIdAsync(id, CancellationToken.None);
            
            if (result.IsFailure)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction("Index");
            }

            var notification = result.Value!;

            // Validate user owns the notification
            if (notification.UserId != userId)
            {
                TempData["Error"] = "You do not have permission to view this notification.";
                return RedirectToAction("Index");
            }

            // Mark as read when viewing details
            if (!notification.IsRead)
            {
                await _notificationService.MarkNotificationAsReadAsync(id, userId);
            }

            return View(notification);
        }
    }
}

