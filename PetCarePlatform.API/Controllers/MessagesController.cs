using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using System.Collections.Generic;

namespace PetCarePlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IUserService _userService;

        public MessagesController(
            IMessageService messageService,
            IUserService userService)
        {
            _messageService = messageService;
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessage(int id)
        {
            var message = await _messageService.GetMessageByIdAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            return Ok(message);
        }

        [HttpGet("conversation/{user1Id}/{user2Id}")]
        public async Task<IActionResult> GetConversation(int user1Id, int user2Id)
        {
            var messages = await _messageService.GetConversationAsync(user1Id, user2Id);
            return Ok(messages);
        }

        [HttpGet("unread/{userId}")]
        public async Task<IActionResult> GetUnreadMessages(int userId)
        {
            var messages = await _messageService.GetUnreadMessagesAsync(userId);
            return Ok(messages);
        }

        //[HttpGet("booking/{bookingId}")]
        //public async Task<IActionResult> GetMessagesByBooking(int bookingId)
        //{
        //    var messages = await _messageService.GetMessagesByBookingAsync(bookingId);
        //    return Ok(messages);
        //}

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var message = new Message
                {
                    SenderId = request.SenderId,
                    ReceiverId = request.ReceiverId,
                    Content = request.Content,
                    BookingId = request.BookingId
                };

                var sentMessage = await _messageService.SendMessageAsync(message);
                return CreatedAtAction(nameof(GetMessage), new { id = sentMessage.Id }, sentMessage);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/mark-read")]
        public async Task<IActionResult> MarkMessageAsRead(int id)
        {
            try
            {
                await _messageService.MarkMessageAsReadAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("mark-all-read/{receiverId}")]
        public async Task<IActionResult> MarkAllMessagesAsRead(int receiverId)
        {
            try
            {
                await _messageService.MarkAllMessagesAsReadAsync(receiverId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("conversation-partners/{userId}")]
        public async Task<IActionResult> GetConversationPartners(int userId)
        {
            try
            {
                var partners = await _messageService.GetConversationPartnersAsync(userId);
                return Ok(partners);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class SendMessageRequest
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Content { get; set; }
        public int? BookingId { get; set; }
    }
}
