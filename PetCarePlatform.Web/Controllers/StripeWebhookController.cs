using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Core.Common;
using Stripe;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PetCarePlatform.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly ILogger<StripeWebhookController> _logger;
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;
        private readonly IEmailService _emailService;
        private readonly string _webhookSecret;

        public StripeWebhookController(
            ILogger<StripeWebhookController> logger,
            IPaymentService paymentService,
            IBookingService bookingService,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _logger = logger;
            _paymentService = paymentService;
            _bookingService = bookingService;
            _emailService = emailService;
            _webhookSecret = configuration["Stripe:WebhookSecret"];
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            
            try
            {
                // Validate webhook secret
                if (string.IsNullOrEmpty(_webhookSecret))
                {
                    _logger.LogError("Webhook secret is not configured");
                    return StatusCode(500, new { error = "Webhook configuration error" });
                }

                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _webhookSecret
                );

                _logger.LogInformation("Received Stripe webhook: {EventType} (ID: {EventId})", 
                    stripeEvent.Type, stripeEvent.Id);

                // Handle the event
                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        await HandlePaymentIntentSucceeded(stripeEvent);
                        break;
                    case "payment_intent.payment_failed":
                        await HandlePaymentIntentFailed(stripeEvent);
                        break;
                    case "payment_intent.requires_action":
                        await HandlePaymentIntentRequiresAction(stripeEvent);
                        break;
                    case "charge.dispute.created":
                        await HandleChargeDisputeCreated(stripeEvent);
                        break;
                    default:
                        _logger.LogInformation("Unhandled event type: {EventType} (ID: {EventId})", 
                            stripeEvent.Type, stripeEvent.Id);
                        break;
                }

                _logger.LogInformation("Successfully processed webhook: {EventType} (ID: {EventId})", 
                    stripeEvent.Type, stripeEvent.Id);
                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook signature verification failed: {StripeError}", ex.Message);
                return BadRequest(new { error = "Invalid webhook signature" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe webhook: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        private async Task HandlePaymentIntentSucceeded(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null)
            {
                _logger.LogWarning("PaymentIntent is null in PaymentIntentSucceeded event");
                return;
            }

            try
            {
                // Find the payment record by transaction ID
                var paymentResult = await _paymentService.GetPaymentByTransactionIdAsync(paymentIntent.Id, CancellationToken.None);
                if (paymentResult.IsFailure || paymentResult.Value == null)
                {
                    _logger.LogWarning("Payment not found for transaction ID: {TransactionId}", paymentIntent.Id);
                    return;
                }

                var payment = paymentResult.Value;

                // Update payment status
                await _paymentService.UpdatePaymentStatusAsync(payment.Id, PaymentStatus.Captured, CancellationToken.None);

                // Confirm booking after payment
                var bookingResult = await _bookingService.ConfirmBookingAfterPaymentAsync(payment.BookingId, payment.Id, CancellationToken.None);
                if (bookingResult.IsSuccess && bookingResult.Value != null)
                {
                    var booking = bookingResult.Value;

                    // Send payment confirmation email
                    try
                    {
                        // Note: Email sending would require getting owner's email from a different source
                        // since BookingResponse doesn't have navigation properties
                        var serviceName = booking.ServiceName ?? "Service";
                        
                        // Email sending would need owner's email from another source
                        _logger.LogInformation("Booking confirmed after payment for booking {BookingId}", payment.BookingId);
                    }
                    catch (Exception emailEx)
                    {
                        // Log email error but don't fail the webhook processing
                        _logger.LogError(emailEx, "Failed to send payment confirmation email for booking {BookingId}", payment.BookingId);
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to confirm booking {BookingId} after payment: {Error}", 
                        payment.BookingId, bookingResult.ErrorMessage);
                }

                _logger.LogInformation("Payment succeeded for booking {BookingId}", payment.BookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling PaymentIntentSucceeded for transaction {TransactionId}", paymentIntent.Id);
            }
        }

        private async Task HandlePaymentIntentFailed(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null)
            {
                _logger.LogWarning("PaymentIntent is null in PaymentIntentFailed event");
                return;
            }

            try
            {
                var paymentResult = await _paymentService.GetPaymentByTransactionIdAsync(paymentIntent.Id, CancellationToken.None);
                if (paymentResult.IsFailure || paymentResult.Value == null)
                {
                    _logger.LogWarning("Payment not found for transaction ID: {TransactionId}", paymentIntent.Id);
                    return;
                }

                var payment = paymentResult.Value;

                // Update payment status
                await _paymentService.UpdatePaymentStatusAsync(payment.Id, PaymentStatus.Failed, CancellationToken.None);

                // Update booking status to Cancelled
                var bookingResult = await _bookingService.GetBookingByIdAsync(payment.BookingId, CancellationToken.None);
                if (bookingResult.IsSuccess && bookingResult.Value != null)
                {
                    await _bookingService.UpdateBookingStatusAsync(bookingResult.Value.Id, BookingStatus.Cancelled, CancellationToken.None);
                    _logger.LogInformation("Booking {BookingId} cancelled due to payment failure", payment.BookingId);
                }

                _logger.LogInformation("Payment failed for booking {BookingId}: {ErrorMessage}", 
                    payment.BookingId, paymentIntent.LastPaymentError?.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling PaymentIntentFailed for transaction {TransactionId}", paymentIntent.Id);
            }
        }

        private async Task HandlePaymentIntentRequiresAction(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null)
            {
                _logger.LogWarning("PaymentIntent is null in PaymentIntentRequiresAction event");
                return;
            }

            try
            {
                var paymentResult = await _paymentService.GetPaymentByTransactionIdAsync(paymentIntent.Id, CancellationToken.None);
                if (paymentResult.IsFailure || paymentResult.Value == null)
                {
                    _logger.LogWarning("Payment not found for transaction ID: {TransactionId}", paymentIntent.Id);
                    return;
                }

                var payment = paymentResult.Value;

                // Update payment status
                await _paymentService.UpdatePaymentStatusAsync(payment.Id, PaymentStatus.RequiresAction, CancellationToken.None);

                _logger.LogInformation("Payment requires action for booking {BookingId}", payment.BookingId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling PaymentIntentRequiresAction for transaction {TransactionId}", paymentIntent.Id);
            }
        }

        private async Task HandleChargeDisputeCreated(Event stripeEvent)
        {
            var dispute = stripeEvent.Data.Object as Dispute;
            if (dispute == null)
            {
                _logger.LogWarning("Dispute is null in ChargeDisputeCreated event");
                return;
            }

            try
            {
                // Find the payment by charge ID
                var paymentResult = await _paymentService.GetPaymentByTransactionIdAsync(dispute.ChargeId, CancellationToken.None);
                if (paymentResult.IsFailure || paymentResult.Value == null)
                {
                    _logger.LogWarning("Payment not found for charge ID: {ChargeId}", dispute.ChargeId);
                    return;
                }

                var payment = paymentResult.Value;

                // Update payment status
                await _paymentService.UpdatePaymentStatusAsync(payment.Id, PaymentStatus.Disputed, CancellationToken.None);

                // Update booking status to Disputed
                var bookingResult = await _bookingService.GetBookingByIdAsync(payment.BookingId, CancellationToken.None);
                if (bookingResult.IsSuccess && bookingResult.Value != null)
                {
                    await _bookingService.UpdateBookingStatusAsync(bookingResult.Value.Id, BookingStatus.Disputed, CancellationToken.None);
                    _logger.LogInformation("Booking {BookingId} marked as disputed: {Reason}", 
                        payment.BookingId, dispute.Reason);
                }

                _logger.LogInformation("Dispute created for booking {BookingId}: {Reason}", 
                    payment.BookingId, dispute.Reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling ChargeDisputeCreated for charge {ChargeId}", dispute.ChargeId);
            }
        }
    }
}
