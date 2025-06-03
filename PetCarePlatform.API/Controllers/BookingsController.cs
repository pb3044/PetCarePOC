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
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IServiceService _serviceService;
        private readonly IPetOwnerService _petOwnerService;

        public BookingsController(
            IBookingService bookingService,
            IServiceService serviceService,
            IPetOwnerService petOwnerService)
        {
            _bookingService = bookingService;
            _serviceService = serviceService;
            _petOwnerService = petOwnerService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBooking(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            return Ok(booking);
        }

        [HttpGet("owner/{ownerId}")]
        public async Task<IActionResult> GetBookingsByOwner(int ownerId, [FromQuery] bool includeHistory = false)
        {
            var bookings = await _bookingService.GetBookingsByOwnerIdAsync(ownerId);
            return Ok(bookings);
        }

        [HttpGet("provider/{providerId}")]
        public async Task<IActionResult> GetBookingsByProvider(int providerId, [FromQuery] bool includeHistory = false)
        {
            var bookings = await _bookingService.GetBookingsByProviderIdAsync(providerId);
            return Ok(bookings);
        }

        [HttpGet("service/{serviceId}")]
        public async Task<IActionResult> GetBookingsByService(int serviceId)
        {
            var bookings = await _bookingService.GetBookingsByServiceIdAsync(serviceId);
            return Ok(bookings);
        }

        [HttpGet("upcoming/{userId}")]
        public async Task<IActionResult> GetUpcomingBookings(int userId)
        {
            var bookings = await _bookingService.GetUpcomingBookingsAsync(userId);
            return Ok(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Check if time slot is available
                if (!await _bookingService.IsTimeSlotAvailableAsync(request.ServiceId, request.StartTime, request.EndTime))
                {
                    return BadRequest("The selected time slot is not available");
                }

                var booking = new Booking
                {
                    ServiceId = request.ServiceId,
                    OwnerId = request.OwnerId,
                    PetId = request.PetId,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Notes = request.Notes,
                    SpecialInstructions = request.SpecialInstructions
                };

                var createdBooking = await _bookingService.CreateBookingAsync(booking);
                return CreatedAtAction(nameof(GetBooking), new { id = createdBooking.Id }, createdBooking);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _bookingService.UpdateBookingStatusAsync(id, request.Status);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(int id, [FromBody] CancelBookingRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _bookingService.CancelBookingAsync(id, request.CancellationReason);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/can-review")]
        public async Task<IActionResult> CanReviewBooking(int id)
        {
            var canReview = await _bookingService.CanBeReviewedAsync(id);
            return Ok(new { canReview });
        }

        [HttpGet("check-availability")]
        public async Task<IActionResult> CheckAvailability(
            [FromQuery] int serviceId,
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime)
        {
            var isAvailable = await _bookingService.IsTimeSlotAvailableAsync(serviceId, startTime, endTime);
            return Ok(new { isAvailable });
        }

        [HttpGet("calculate-price")]
        public async Task<IActionResult> CalculatePrice(
            [FromQuery] int serviceId,
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime,
            [FromQuery] int petId)
        {
            try
            {
                var price = await _bookingService.CalculateBookingPriceAsync(serviceId, startTime, endTime, petId);
                return Ok(new { price });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class CreateBookingRequest
    {
        public int ServiceId { get; set; }
        public int OwnerId { get; set; }
        public int? PetId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Notes { get; set; }
        public string SpecialInstructions { get; set; }
    }

    public class UpdateBookingStatusRequest
    {
        public BookingStatus Status { get; set; }
    }

    public class CancelBookingRequest
    {
        public string CancellationReason { get; set; }
    }
}
