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
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;

        public PaymentsController(
            IPaymentService paymentService,
            IBookingService bookingService)
        {
            _paymentService = paymentService;
            _bookingService = bookingService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var payments = await _paymentService.GetPaymentsByUserIdAsync(userId);
            return View(payments);
        }

        public async Task<IActionResult> Details(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        [HttpGet]
        public async Task<IActionResult> ProcessPayment(int bookingId)
        {
            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            if (booking == null)
            {
                return NotFound();
            }

            var model = new ProcessPaymentViewModel
            {
                BookingId = bookingId,
                Amount = booking.TotalPrice,
                ServiceName = booking.Service?.Title ?? "Service"
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(ProcessPaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Create payment intent
                var payment = await _paymentService.CreatePaymentIntentAsync(model.BookingId);
                
                // In a real application, you would integrate with a payment processor here
                // For now, we'll simulate a successful payment
                var confirmedPayment = await _paymentService.ConfirmPaymentAsync(payment.Id, "sim_" + Guid.NewGuid().ToString());

                TempData["SuccessMessage"] = "Payment processed successfully!";
                return RedirectToAction("Details", new { id = confirmedPayment.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while processing the payment: " + ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> Receipt(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        [HttpPost]
        public async Task<IActionResult> RequestRefund(int id, string reason)
        {
            try
            {
                var payment = await _paymentService.GetPaymentByIdAsync(id);
                if (payment == null)
                {
                    return NotFound();
                }

                await _paymentService.ProcessRefundAsync(id, payment.Amount, reason);
                TempData["SuccessMessage"] = "Refund request submitted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while processing the refund: " + ex.Message;
            }

            return RedirectToAction("Details", new { id });
        }

        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Earnings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var earnings = await _paymentService.GetProviderEarningsAsync(userId);
            ViewBag.TotalEarnings = earnings;
            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Revenue()
        {
            var revenue = await _paymentService.GetTotalRevenueAsync();
            ViewBag.TotalRevenue = revenue;
            return View();
        }
    }
}

