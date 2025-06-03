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
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNotification(int id)
        {
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            return Ok(notification);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetNotificationsByUser(int userId)
        {
            var notifications = await _notificationService.GetNotificationsByUserIdAsync(userId);
            return Ok(notifications);
        }

        [HttpGet("unread/{userId}")]
        public async Task<IActionResult> GetUnreadNotifications(int userId)
        {
            var notifications = await _notificationService.GetUnreadNotificationsByUserIdAsync(userId);
            return Ok(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var notification = new Notification
                {
                    UserId = request.UserId,
                    Title = request.Title,
                    Content = request.Content,
                    Type = request.Type,
                    ActionUrl = request.ActionUrl
                };

                var createdNotification = await _notificationService.CreateNotificationAsync(notification);
                return CreatedAtAction(nameof(GetNotification), new { id = createdNotification.Id }, createdNotification);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/mark-read")]
        public async Task<IActionResult> MarkNotificationAsRead(int id)
        {
            try
            {
                await _notificationService.MarkNotificationAsReadAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("mark-all-read/{userId}")]
        public async Task<IActionResult> MarkAllNotificationsAsRead(int userId)
        {
            try
            {
                await _notificationService.MarkAllNotificationsAsReadAsync(userId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("booking-request/{bookingId}")]
        public async Task<IActionResult> SendBookingRequestNotification(int bookingId)
        {
            try
            {
                await _notificationService.SendBookingRequestNotificationAsync(bookingId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("booking-confirmation/{bookingId}")]
        public async Task<IActionResult> SendBookingConfirmationNotification(int bookingId)
        {
            try
            {
                await _notificationService.SendBookingConfirmationNotificationAsync(bookingId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("booking-cancellation/{bookingId}")]
        public async Task<IActionResult> SendBookingCancellationNotification(int bookingId)
        {
            try
            {
                await _notificationService.SendBookingCancellationNotificationAsync(bookingId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("payment-confirmation/{paymentId}")]
        public async Task<IActionResult> SendPaymentConfirmationNotification(int paymentId)
        {
            try
            {
                await _notificationService.SendPaymentConfirmationNotificationAsync(paymentId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("new-message/{messageId}")]
        public async Task<IActionResult> SendNewMessageNotification(int messageId)
        {
            try
            {
                await _notificationService.SendNewMessageNotificationAsync(messageId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("new-review/{reviewId}")]
        public async Task<IActionResult> SendNewReviewNotification(int reviewId)
        {
            try
            {
                await _notificationService.SendNewReviewNotificationAsync(reviewId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class CreateNotificationRequest
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public NotificationType Type { get; set; }
        public string ActionUrl { get; set; }
    }
}
