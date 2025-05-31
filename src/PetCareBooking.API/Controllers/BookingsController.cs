using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Core.Models;
using PetCareBooking.Services.Interfaces;
using System.Threading.Tasks;

namespace PetCareBooking.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
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

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetBookingsByCustomer(string customerId)
        {
            var bookings = await _bookingService.GetBookingsByCustomerIdAsync(customerId);
            return Ok(bookings);
        }

        [HttpGet("provider/{providerId}")]
        public async Task<IActionResult> GetBookingsByProvider(string providerId)
        {
            var bookings = await _bookingService.GetBookingsByProviderIdAsync(providerId);
            return Ok(bookings);
        }

        [HttpGet("pet/{petId}")]
        public async Task<IActionResult> GetBookingsByPet(int petId)
        {
            var bookings = await _bookingService.GetBookingsByPetIdAsync(petId);
            return Ok(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking(Booking booking)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdBooking = await _bookingService.CreateBookingAsync(booking);
            return CreatedAtAction(nameof(GetBooking), new { id = createdBooking.Id }, createdBooking);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] string status)
        {
            var booking = await _bookingService.UpdateBookingStatusAsync(id, status);
            if (booking == null)
            {
                return NotFound();
            }
            return Ok(booking);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var result = await _bookingService.DeleteBookingAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPost("calculate-taxes")]
        public async Task<IActionResult> CalculateBookingTaxes(Booking booking, [FromQuery] string provinceCode)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var bookingWithTaxes = await _bookingService.CalculateBookingTaxesAsync(booking, provinceCode);
                return Ok(bookingWithTaxes);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
