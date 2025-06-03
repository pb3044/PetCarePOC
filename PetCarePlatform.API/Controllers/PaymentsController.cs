using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Infrastructure.Payment;
using Stripe;
using Microsoft.Extensions.Configuration;

namespace PetCarePlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly Core.Interfaces.IPaymentService _paymentService;
        private readonly IConfiguration _configuration;

        public PaymentsController(
            Core.Interfaces.IPaymentService paymentService,
            IConfiguration configuration)
        {
            _paymentService = paymentService;
            _configuration = configuration;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPayment(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            return Ok(payment);
        }

        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetPaymentByBooking(int bookingId)
        {
            var payment = await _paymentService.GetPaymentByBookingIdAsync(bookingId);
            if (payment == null)
            {
                return NotFound();
            }

            return Ok(payment);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPaymentsByUser(int userId)
        {
            var payments = await _paymentService.GetPaymentsByUserIdAsync(userId);
            return Ok(payments);
        }

        [HttpPost("create-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var payment = await _paymentService.CreatePaymentIntentAsync(request.BookingId);
                return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> ConfirmPayment(int id, [FromBody] ConfirmPaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var payment = await _paymentService.ConfirmPaymentAsync(id, request.TransactionId);
                return Ok(payment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/refund")]
        public async Task<IActionResult> ProcessRefund(int id, [FromBody] ProcessRefundRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var payment = await _paymentService.ProcessRefundAsync(id, request.Amount, request.Reason);
                return Ok(payment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> HandleStripeWebhook()
        {
            var json = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _configuration["Stripe:WebhookSecret"]
                );

                // Handle the event based on its type
                //if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                //{
                //    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                //    // Handle successful payment
                //    // In a real implementation, you would update the payment status in your database
                //    Console.WriteLine($"Payment succeeded: {paymentIntent.Id}");
                //}
                //else if (stripeEvent.Type == Events.PaymentIntentPaymentFailed)
                //{
                //    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                //    // Handle failed payment
                //    Console.WriteLine($"Payment failed: {paymentIntent.Id}");
                //}
                // Add more event types as needed

                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("client-secret")]
        public IActionResult GetClientSecret()
        {
            return Ok(new { publishableKey = _configuration["Stripe:PublishableKey"] });
        }
    }

    public class CreatePaymentIntentRequest
    {
        public int BookingId { get; set; }
    }

    public class ConfirmPaymentRequest
    {
        public string TransactionId { get; set; }
    }

    public class ProcessRefundRequest
    {
        public decimal Amount { get; set; }
        public string Reason { get; set; }
    }
}
