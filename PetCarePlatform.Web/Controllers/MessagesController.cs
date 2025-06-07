using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
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

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var unreadMessages = await _messageService.GetUnreadMessagesAsync(userId);
            return View(unreadMessages);
        }

        public async Task<IActionResult> Details(int id)
        {
            var message = await _messageService.GetMessageByIdAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            // Mark as read if current user is the receiver
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (message.ReceiverId == userId && !message.IsRead)
            {
                await _messageService.MarkMessageAsReadAsync(id);
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

            try
            {
                var senderId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                
                var message = new Message
                {
                    SenderId = senderId,
                    ReceiverId = model.ReceiverId,
                    Content = model.Content,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                await _messageService.SendMessageAsync(message);
                TempData["SuccessMessage"] = "Message sent successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while sending the message: " + ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> Conversation(int otherUserId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var messages = await _messageService.GetConversationAsync(userId, otherUserId);
            ViewBag.OtherUserId = otherUserId;
            return View(messages);
        }

        public async Task<IActionResult> ConversationPartners()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var partners = await _messageService.GetConversationPartnersAsync(userId);
            return View(partners);
        }
    }
}

