using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using PetCarePlatform.Core.DTOs.Queries;
using PetCarePlatform.Core.DTOs.Requests;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Web.Models;
using System.Security.Claims;

namespace PetCarePlatform.Web.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly IMessageService _messageService;

        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Get unread message count
            var unreadCountResult = await _messageService.GetUnreadMessageCountAsync(userId);
            ViewBag.UnreadCount = unreadCountResult.IsSuccess ? unreadCountResult.Value : 0;

            // Get conversation summaries
            var summariesResult = await _messageService.GetConversationSummariesAsync(userId);
            if (summariesResult.IsFailure)
            {
                TempData["Error"] = summariesResult.ErrorMessage;
                return View(new List<PetCarePlatform.Core.DTOs.Responses.ConversationSummaryResponse>());
            }

            return View(summariesResult.Value);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var result = await _messageService.GetMessageByIdAsync(id, CancellationToken.None);
            if (result.IsFailure)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction("Index");
            }

            var message = result.Value!;

            // Mark as read if current user is the receiver
            if (message.ReceiverId == userId && !message.IsRead)
            {
                await _messageService.MarkMessageAsReadAsync(id, userId, CancellationToken.None);
            }

            return View(message);
        }

        [HttpGet]
        public IActionResult Compose(int? receiverId)
        {
            var model = new ComposeMessageViewModel();
            if (receiverId.HasValue)
            {
                model.ReceiverId = receiverId.Value;
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Compose(ComposeMessageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var senderId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var request = new SendMessageRequest
            {
                ReceiverId = model.ReceiverId,
                BookingId = model.BookingId,
                Content = model.Content
            };

            var result = await _messageService.SendMessageAsync(request, senderId);

            if (result.IsFailure)
            {
                ModelState.AddModelError("", result.ErrorMessage);
                return View(model);
            }

            TempData["SuccessMessage"] = "Message sent successfully!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Conversation(int otherUserId, int page = 1, int pageSize = 50)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var result = await _messageService.GetConversationAsync(userId, otherUserId, page, pageSize);
            
            if (result.IsFailure)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction("Index");
            }

            ViewBag.OtherUserId = otherUserId;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = result.Value!.TotalPages;
            ViewBag.TotalCount = result.Value.TotalCount;

            return View(result.Value.Items);
        }

        public async Task<IActionResult> ConversationPartners()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Use conversation summaries instead
            var result = await _messageService.GetConversationSummariesAsync(userId);
            
            if (result.IsFailure)
            {
                TempData["Error"] = result.ErrorMessage;
                return View(new List<PetCarePlatform.Core.DTOs.Responses.ConversationSummaryResponse>());
            }

            return View(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int messageId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var result = await _messageService.MarkMessageAsReadAsync(messageId, userId);
            
            if (result.IsFailure)
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }

            return Json(new { success = true, message = "Message marked as read" });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var result = await _messageService.MarkAllMessagesAsReadAsync(userId, userId);
            
            if (result.IsFailure)
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }

            return Json(new { success = true, message = "All messages marked as read" });
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var result = await _messageService.GetUnreadMessageCountAsync(userId);
            
            if (result.IsFailure)
            {
                return Json(new { success = false, count = 0 });
            }

            return Json(new { success = true, count = result.Value });
        }
    }
}

