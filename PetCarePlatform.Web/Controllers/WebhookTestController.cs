using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PetCarePlatform.Core.Interfaces;
using Stripe;
using System;
using System.Threading.Tasks;

namespace PetCarePlatform.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookTestController : ControllerBase
    {
        private readonly ILogger<WebhookTestController> _logger;
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;

        public WebhookTestController(
            ILogger<WebhookTestController> logger,
            IPaymentService paymentService,
            IConfiguration configuration)
        {
            _logger = logger;
            _paymentService = paymentService;
            _configuration = configuration;
        }

        [HttpGet("status")]
        public IActionResult GetWebhookStatus()
        {
            var webhookSecret = _configuration["Stripe:WebhookSecret"];
            var secretKey = _configuration["Stripe:SecretKey"];
            var publishableKey = _configuration["Stripe:PublishableKey"];

            return Ok(new
            {
                webhookSecretConfigured = !string.IsNullOrEmpty(webhookSecret),
                secretKeyConfigured = !string.IsNullOrEmpty(secretKey),
                publishableKeyConfigured = !string.IsNullOrEmpty(publishableKey),
                webhookEndpoint = $"{Request.Scheme}://{Request.Host}/api/StripeWebhook",
                timestamp = DateTime.UtcNow
            });
        }

        [HttpPost("test-payment-success")]
        public async Task<IActionResult> TestPaymentSuccess([FromBody] TestWebhookRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.TransactionId))
                {
                    return BadRequest(new { error = "TransactionId is required" });
                }

                // Simulate a successful payment webhook
                var paymentResult = await _paymentService.GetPaymentByTransactionIdAsync(request.TransactionId);
                if (paymentResult.IsFailure || paymentResult.Value == null)
                {
                    return NotFound(new { error = "Payment not found" });
                }

                var payment = paymentResult.Value;

                // Update payment status to simulate webhook processing
                var updateResult = await _paymentService.UpdatePaymentStatusAsync(payment.Id, Core.Models.PaymentStatus.Captured);
                if (updateResult.IsFailure)
                {
                    return StatusCode(500, new { error = updateResult.ErrorMessage });
                }

                _logger.LogInformation("Test webhook: Payment {PaymentId} marked as successful", payment.Id);

                return Ok(new
                {
                    message = "Test webhook processed successfully",
                    paymentId = payment.Id,
                    transactionId = request.TransactionId,
                    status = "succeeded"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing test webhook");
                return StatusCode(500, new { error = "Test webhook failed" });
            }
        }

        [HttpPost("test-payment-failure")]
        public async Task<IActionResult> TestPaymentFailure([FromBody] TestWebhookRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.TransactionId))
                {
                    return BadRequest(new { error = "TransactionId is required" });
                }

                // Simulate a failed payment webhook
                var paymentResult = await _paymentService.GetPaymentByTransactionIdAsync(request.TransactionId);
                if (paymentResult.IsFailure || paymentResult.Value == null)
                {
                    return NotFound(new { error = "Payment not found" });
                }

                var payment = paymentResult.Value;

                // Update payment status to simulate webhook processing
                var updateResult = await _paymentService.UpdatePaymentStatusAsync(payment.Id, Core.Models.PaymentStatus.Failed);
                if (updateResult.IsFailure)
                {
                    return StatusCode(500, new { error = updateResult.ErrorMessage });
                }

                _logger.LogInformation("Test webhook: Payment {PaymentId} marked as failed", payment.Id);

                return Ok(new
                {
                    message = "Test webhook processed successfully",
                    paymentId = payment.Id,
                    transactionId = request.TransactionId,
                    status = "failed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing test webhook");
                return StatusCode(500, new { error = "Test webhook failed" });
            }
        }

        [HttpGet("stripe-cli-command")]
        public IActionResult GetStripeCliCommand()
        {
            var webhookUrl = $"{Request.Scheme}://{Request.Host}/api/StripeWebhook";
            
            return Ok(new
            {
                command = $"stripe listen --forward-to {webhookUrl}",
                webhookUrl = webhookUrl,
                instructions = new[]
                {
                    "1. Install Stripe CLI from https://stripe.com/docs/stripe-cli",
                    "2. Run: stripe login",
                    "3. Run the command above to start webhook forwarding",
                    "4. Copy the webhook secret from the CLI output",
                    "5. Update your appsettings.json with the webhook secret"
                }
            });
        }
    }

    public class TestWebhookRequest
    {
        public string TransactionId { get; set; }
    }
}
