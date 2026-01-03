using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading;
using System.Threading.Tasks;
using PetCarePlatform.Core.DTOs.Requests;
using PetCarePlatform.Core.DTOs.Queries;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using System.Security.Claims;

namespace PetCarePlatform.Web.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPetOwnerService _petOwnerService;

        public BookingsController(
            IBookingService bookingService,
            IBookingRepository bookingRepository,
            IPetOwnerService petOwnerService)
        {
            _bookingService = bookingService;
            _bookingRepository = bookingRepository;
            _petOwnerService = petOwnerService;
        }

        [HttpGet]
        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
            {
                TempData["Error"] = "Pet owner not found.";
                return RedirectToAction("Index", "PetOwner");
            }

            var petOwner = petOwnerResult.Value;

            // Use repository to get full Booking entity with navigation properties
            var booking = await _bookingRepository.GetByIdAsync(id);
            
            if (booking == null)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToAction("MyBookings", "PetOwner");
            }

            // Validate user owns the booking
            if (booking.OwnerId != petOwner.Id)
            {
                TempData["Error"] = "You do not have permission to view this booking.";
                return RedirectToAction("MyBookings", "PetOwner");
            }

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> Cancel(int id, string cancellationReason = "")
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
            if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
            {
                return Json(new { success = false, message = "Pet owner not found." });
            }

            var petOwner = petOwnerResult.Value;

            // Verify booking exists and belongs to user
            var bookingResult = await _bookingService.GetBookingByIdAsync(id, CancellationToken.None);
            if (bookingResult.IsFailure)
            {
                return Json(new { success = false, message = bookingResult.ErrorMessage });
            }

            var booking = bookingResult.Value!;
            if (booking.OwnerId != petOwner.Id)
            {
                return Json(new { success = false, message = "Booking not found or doesn't belong to you." });
            }

            // Use enterprise pattern method
            var request = new CancelBookingRequest
            {
                BookingId = id,
                CancellationReason = cancellationReason
            };

            var result = await _bookingService.CancelBookingAsync(request);

            if (result.IsFailure)
            {
                return Json(new { success = false, message = result.ErrorMessage });
            }

            TempData["SuccessMessage"] = "Booking cancelled successfully.";
            return Json(new { success = true, message = "Booking cancelled successfully." });
        }

        [HttpGet]
        [Authorize(Roles = "PetOwner,ServiceProvider")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, BookingStatus? status = null, bool? upcomingOnly = null)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var query = new BookingQuery
            {
                PageNumber = page,
                PageSize = pageSize,
                Status = status,
                UpcomingOnly = upcomingOnly,
                SortBy = "StartTime",
                SortOrder = "asc"
            };

            // Set owner or provider filter based on role
            if (userRole == "PetOwner")
            {
                var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
                if (petOwnerResult.IsSuccess && petOwnerResult.Value != null)
                {
                    query.OwnerId = petOwnerResult.Value.Id;
                }
            }
            else if (userRole == "ServiceProvider")
            {
                // For service providers, we'd need to get their provider ID
                // For now, we'll use ProviderId filter if needed
                query.ProviderId = null; // TODO: Get provider ID from service
            }

            var result = await _bookingService.GetBookingsAsync(query);

            if (result.IsFailure)
            {
                TempData["Error"] = result.ErrorMessage;
                return View(new PetCarePlatform.Core.Common.PagedResult<PetCarePlatform.Core.DTOs.Responses.BookingResponse>(
                    new List<PetCarePlatform.Core.DTOs.Responses.BookingResponse>(),
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
    }
}
