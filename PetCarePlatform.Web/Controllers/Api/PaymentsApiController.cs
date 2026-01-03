using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.DTOs.Requests;
using PetCarePlatform.Core.Models;
using Stripe;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace PetCarePlatform.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsApiController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentsApiController> _logger;

        public PaymentsApiController(
            IPaymentService paymentService,
            IBookingService bookingService,
            IConfiguration configuration,
            ILogger<PaymentsApiController> logger)
        {
            _paymentService = paymentService;
            _bookingService = bookingService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { error = "Invalid request data" });
                }

                // Verify the booking exists and belongs to the user
                var bookingResult = await _bookingService.GetBookingByIdAsync(request.BookingId, CancellationToken.None);
                if (bookingResult.IsFailure)
                {
                    return NotFound(new { error = bookingResult.ErrorMessage });
                }

                var booking = bookingResult.Value!;
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (booking.OwnerId != userId)
                {
                    return Forbid("You can only create payments for your own bookings");
                }

                // Create payment intent
                var createRequest = new PetCarePlatform.Core.DTOs.Requests.CreatePaymentIntentRequest { BookingId = request.BookingId };
                var paymentResult = await _paymentService.CreatePaymentIntentAsync(createRequest, CancellationToken.None);
                if (paymentResult.IsFailure)
                {
                    return BadRequest(new { error = paymentResult.ErrorMessage });
                }

                var payment = paymentResult.Value!;

                // Get the Stripe payment intent to return client secret
                var service = new PaymentIntentService();
                var intent = await service.GetAsync(payment.TransactionId);

                return Ok(new
                {
                    clientSecret = intent.ClientSecret,
                    paymentId = payment.Id,
                    amount = payment.Amount,
                    currency = "cad"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating payment intent for booking {BookingId}", request.BookingId);
                return BadRequest(new { error = ex.Message });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error when creating payment intent for booking {BookingId}", request.BookingId);
                return StatusCode(500, new { error = "Payment service error" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when creating payment intent for booking {BookingId}", request.BookingId);
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }

        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { error = "Invalid request data" });
                }

                // Verify the payment exists and belongs to the user
                var paymentResult = await _paymentService.GetPaymentByIdAsync(request.PaymentId, CancellationToken.None);
                if (paymentResult.IsFailure)
                {
                    return NotFound(new { error = paymentResult.ErrorMessage });
                }

                var payment = paymentResult.Value!;
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (payment.UserId != userId)
                {
                    return Forbid("You can only confirm your own payments");
                }

                // Confirm the payment
                var confirmRequest = new PetCarePlatform.Core.DTOs.Requests.ConfirmPaymentRequest 
                { 
                    PaymentId = request.PaymentId, 
                    TransactionId = request.TransactionId 
                };
                var confirmedPaymentResult = await _paymentService.ConfirmPaymentAsync(confirmRequest, CancellationToken.None);
                if (confirmedPaymentResult.IsFailure)
                {
                    return BadRequest(new { error = confirmedPaymentResult.ErrorMessage });
                }

                var confirmedPayment = confirmedPaymentResult.Value!;

                return Ok(new
                {
                    success = true,
                    paymentId = confirmedPayment.Id,
                    status = confirmedPayment.Status.ToString(),
                    bookingId = confirmedPayment.BookingId
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when confirming payment {PaymentId}", request.PaymentId);
                return BadRequest(new { error = ex.Message });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error when confirming payment {PaymentId}", request.PaymentId);
                return StatusCode(500, new { error = "Payment service error" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when confirming payment {PaymentId}", request.PaymentId);
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }

        [HttpGet("payment-status/{paymentId}")]
        public async Task<IActionResult> GetPaymentStatus(int paymentId)
        {
            try
            {
                var paymentResult = await _paymentService.GetPaymentByIdAsync(paymentId, CancellationToken.None);
                if (paymentResult.IsFailure)
                {
                    return NotFound(new { error = paymentResult.ErrorMessage });
                }

                var payment = paymentResult.Value!;
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (payment.UserId != userId)
                {
                    return Forbid("You can only view your own payments");
                }

                return Ok(new
                {
                    paymentId = payment.Id,
                    status = payment.Status.ToString(),
                    amount = payment.Amount,
                    currency = "cad",
                    transactionId = payment.TransactionId,
                    createdAt = payment.CreatedAt,
                    updatedAt = payment.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status for payment {PaymentId}", paymentId);
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }

        [HttpPost("request-refund")]
        public async Task<IActionResult> RequestRefund([FromBody] RefundRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { error = "Invalid request data" });
                }

                // Verify the payment exists and belongs to the user
                var paymentResult = await _paymentService.GetPaymentByIdAsync(request.PaymentId, CancellationToken.None);
                if (paymentResult.IsFailure)
                {
                    return NotFound(new { error = paymentResult.ErrorMessage });
                }

                var payment = paymentResult.Value!;
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                if (payment.UserId != userId)
                {
                    return Forbid("You can only request refunds for your own payments");
                }

                // Process refund
                var refundRequest = new ProcessRefundRequest
                {
                    PaymentId = request.PaymentId,
                    Amount = request.Amount ?? payment.Amount,
                    Reason = request.Reason
                };
                var refundedPaymentResult = await _paymentService.ProcessRefundAsync(refundRequest, CancellationToken.None);
                if (refundedPaymentResult.IsFailure)
                {
                    return BadRequest(new { error = refundedPaymentResult.ErrorMessage });
                }

                var refundedPayment = refundedPaymentResult.Value!;

                return Ok(new
                {
                    success = true,
                    paymentId = refundedPayment.Id,
                    status = refundedPayment.Status.ToString(),
                    refundAmount = request.Amount ?? payment.Amount
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when processing refund for payment {PaymentId}", request.PaymentId);
                return BadRequest(new { error = ex.Message });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error when processing refund for payment {PaymentId}", request.PaymentId);
                return StatusCode(500, new { error = "Payment service error" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when processing refund for payment {PaymentId}", request.PaymentId);
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }

        [HttpGet("publishable-key")]
        public IActionResult GetPublishableKey()
        {
            var publishableKey = _configuration["Stripe:PublishableKey"];
            if (string.IsNullOrEmpty(publishableKey))
            {
                return StatusCode(500, new { error = "Stripe configuration missing" });
            }

            return Ok(new { publishableKey });
        }
    }

    public class CreatePaymentIntentRequest
    {
        public int BookingId { get; set; }
    }

    public class ConfirmPaymentRequest
    {
        public int PaymentId { get; set; }
        public string TransactionId { get; set; }
    }

    public class RefundRequest
    {
        public int PaymentId { get; set; }
        public decimal? Amount { get; set; }
        public string Reason { get; set; }
    }
}
