using Microsoft.AspNetCore.Mvc;
using PetCareBooking.Core.Models;
using PetCareBooking.Services.Interfaces;
using System.Threading.Tasks;

namespace PetCareBooking.Web.Controllers
{
    public class BookingsController : Controller
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public async Task<IActionResult> Index(string customerId)
        {
            if (string.IsNullOrEmpty(customerId))
            {
                return View(new System.Collections.Generic.List<Booking>());
            }

            var bookings = await _bookingService.GetBookingsByCustomerIdAsync(customerId);
            return View(bookings);
        }

        public async Task<IActionResult> Details(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (ModelState.IsValid)
            {
                await _bookingService.CreateBookingAsync(booking);
                return RedirectToAction(nameof(Index), new { customerId = booking.CustomerId });
            }
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var booking = await _bookingService.UpdateBookingStatusAsync(id, status);
            if (booking == null)
            {
                return NotFound();
            }
            return RedirectToAction(nameof(Details), new { id = booking.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            await _bookingService.DeleteBookingAsync(id);
            return RedirectToAction(nameof(Index), new { customerId = booking.CustomerId });
        }
    }
}
