using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading;
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
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;
        private readonly IReceiptService _receiptService;

        public PaymentsController(
            IPaymentService paymentService,
            IBookingService bookingService,
            IReceiptService receiptService)
        {
            _paymentService = paymentService;
            _bookingService = bookingService;
            _receiptService = receiptService;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var query = new PaymentQuery
            {
                UserId = userId,
                PageNumber = page,
                PageSize = pageSize,
                SortBy = "CreatedAt",
                SortOrder = "desc"
            };

            var result = await _paymentService.GetPaymentsAsync(query, CancellationToken.None);
            
            if (result.IsFailure)
            {
                TempData["Error"] = result.ErrorMessage;
                return View(new PetCarePlatform.Core.Common.PagedResult<PetCarePlatform.Core.DTOs.Responses.PaymentResponse>(
                    new System.Collections.Generic.List<PetCarePlatform.Core.DTOs.Responses.PaymentResponse>(),
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

        public async Task<IActionResult> Details(int id)
        {
            var result = await _paymentService.GetPaymentByIdAsync(id, CancellationToken.None);
            
            if (result.IsFailure)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction("Index");
            }

            var payment = result.Value!;

            // Verify user has access to this payment
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (payment.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(payment);
        }

        [HttpGet]
        public async Task<IActionResult> ProcessPayment(int bookingId)
        {
            var bookingResult = await _bookingService.GetBookingByIdAsync(bookingId);
            if (bookingResult.IsFailure)
            {
                TempData["Error"] = bookingResult.ErrorMessage;
                return RedirectToAction("Index", "Bookings");
            }

            var booking = bookingResult.Value!;

            // Check if payment already exists
            var existingPaymentResult = await _paymentService.GetPaymentByBookingIdAsync(bookingId, CancellationToken.None);
            if (existingPaymentResult.IsSuccess && existingPaymentResult.Value != null)
            {
                var existingPayment = existingPaymentResult.Value;
                if (existingPayment.Status == PaymentStatus.Captured)
                {
                    TempData["InfoMessage"] = "Payment has already been processed for this booking.";
                    return RedirectToAction("Receipt", new { id = existingPayment.Id });
                }
                else if (existingPayment.Status == PaymentStatus.Pending)
                {
                    TempData["InfoMessage"] = "Payment is already in progress for this booking.";
                    return RedirectToAction("Details", new { id = existingPayment.Id });
                }
            }

            var model = new ProcessPaymentViewModel
            {
                BookingId = bookingId,
                Amount = booking.TotalPrice,
                ServiceName = booking.ServiceName
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

            // Create payment intent
            var createRequest = new CreatePaymentIntentRequest
            {
                BookingId = model.BookingId
            };

            var createResult = await _paymentService.CreatePaymentIntentAsync(createRequest, CancellationToken.None);
            if (createResult.IsFailure)
            {
                ModelState.AddModelError("", createResult.ErrorMessage);
                return View(model);
            }

            var payment = createResult.Value!;
            
            // In a real application, you would integrate with a payment processor here
            // For now, we'll simulate a successful payment
            var confirmRequest = new ConfirmPaymentRequest
            {
                PaymentId = payment.Id,
                TransactionId = "sim_" + Guid.NewGuid().ToString()
            };

            var confirmResult = await _paymentService.ConfirmPaymentAsync(confirmRequest, CancellationToken.None);
            if (confirmResult.IsFailure)
            {
                ModelState.AddModelError("", confirmResult.ErrorMessage);
                return View(model);
            }

            TempData["SuccessMessage"] = "Payment processed successfully!";
            return RedirectToAction("Details", new { id = confirmResult.Value!.Id });
        }

        public async Task<IActionResult> Receipt(int id)
        {
            var result = await _paymentService.GetPaymentByIdAsync(id, CancellationToken.None);
            if (result.IsFailure)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction("Index");
            }

            var payment = result.Value!;

            // Verify user has access to this payment
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (payment.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Generate receipt from payment response
            // Note: ReceiptService may need to be updated to work with PaymentResponse
            // For now, we'll pass the payment response
            var receiptHtml = await _receiptService.GenerateReceiptFromPaymentResponseAsync(payment);
            ViewBag.ReceiptHtml = receiptHtml;

            return View(payment);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadReceipt(int id)
        {
            var result = await _paymentService.GetPaymentByIdAsync(id, CancellationToken.None);
            if (result.IsFailure)
            {
                return NotFound();
            }

            var payment = result.Value!;

            // Verify user has access to this payment
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (payment.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var receiptBytes = await _receiptService.GenerateReceiptPdfFromPaymentResponseAsync(payment);
            var receiptNumber = $"RCP-{payment.Id:D6}-{payment.CreatedAt:yyyyMMdd}";
            
            return File(receiptBytes, "text/html", $"{receiptNumber}.html");
        }

        [HttpPost]
        public async Task<IActionResult> RequestRefund(int id, string reason)
        {
            var paymentResult = await _paymentService.GetPaymentByIdAsync(id, CancellationToken.None);
            if (paymentResult.IsFailure)
            {
                TempData["ErrorMessage"] = paymentResult.ErrorMessage;
                return RedirectToAction("Index");
            }

            var payment = paymentResult.Value!;

            var refundRequest = new ProcessRefundRequest
            {
                PaymentId = id,
                Amount = payment.Amount,
                Reason = reason
            };

            var refundResult = await _paymentService.ProcessRefundAsync(refundRequest, CancellationToken.None);
            if (refundResult.IsFailure)
            {
                TempData["ErrorMessage"] = refundResult.ErrorMessage;
            }
            else
            {
                TempData["SuccessMessage"] = "Refund request submitted successfully!";
            }

            return RedirectToAction("Details", new { id });
        }

        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Earnings()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _paymentService.GetProviderEarningsAsync(userId, null, null, CancellationToken.None);
            
            if (result.IsFailure)
            {
                TempData["Error"] = result.ErrorMessage;
                ViewBag.TotalEarnings = 0m;
            }
            else
            {
                ViewBag.TotalEarnings = result.Value;
            }
            
            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Revenue()
        {
            var result = await _paymentService.GetTotalRevenueAsync(null, null, CancellationToken.None);
            
            if (result.IsFailure)
            {
                TempData["Error"] = result.ErrorMessage;
                ViewBag.TotalRevenue = 0m;
            }
            else
            {
                ViewBag.TotalRevenue = result.Value;
            }
            
            return View();
        }
    }
}

