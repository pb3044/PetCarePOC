using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Infrastructure.Configuration;
using System.Net;
using System.Net.Mail;

namespace PetCarePlatform.Infrastructure.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailConfiguration> emailConfig, ILogger<EmailService> logger)
        {
            _emailConfig = emailConfig?.Value ?? throw new ArgumentNullException(nameof(emailConfig));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                // Check if email is enabled
                if (!_emailConfig.IsEnabled)
                {
                    _logger.LogInformation("Email sending is disabled. Email would be sent to: {To}, Subject: {Subject}", to, subject);
                    return;
                }

                // Validate email configuration
                if (string.IsNullOrWhiteSpace(_emailConfig.SmtpHost))
                {
                    _logger.LogWarning("SMTP host is not configured. Email not sent to: {To}", to);
                    return;
                }

                if (string.IsNullOrWhiteSpace(_emailConfig.Username) || string.IsNullOrWhiteSpace(_emailConfig.Password))
                {
                    _logger.LogWarning("SMTP credentials are not configured. Email not sent to: {To}. Please configure Email:Username and Email:Password in User Secrets or environment variables.", to);
                    return;
                }

                _logger.LogInformation("Sending email to {To} with subject: {Subject}", to, subject);

                using var client = new SmtpClient(_emailConfig.SmtpHost, _emailConfig.SmtpPort);
                client.EnableSsl = _emailConfig.EnableSsl;
                client.Credentials = new NetworkCredential(_emailConfig.Username, _emailConfig.Password);
                client.Timeout = 30000; // 30 seconds timeout

                using var message = new MailMessage();
                message.From = new MailAddress(_emailConfig.FromEmail, _emailConfig.FromName);
                message.To.Add(to);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = isHtml;

                await client.SendMailAsync(message);
                
                _logger.LogInformation("Email successfully sent to {To}", to);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "SMTP error sending email to {To}: {SmtpError}", to, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
        }

        public async Task SendBookingConfirmationAsync(string to, string petOwnerName, string serviceName, DateTime serviceDate, decimal amount)
        {
            var subject = "Booking Confirmed - PetCare Platform";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 10px;'>
                        <h2 style='color: #007bff; text-align: center;'>🐾 Booking Confirmed!</h2>
                        
                        <p>Dear {petOwnerName},</p>
                        
                        <p>Great news! Your booking has been confirmed.</p>
                        
                        <div style='background-color: white; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                            <h3 style='color: #333; margin-top: 0;'>Booking Details</h3>
                            <p><strong>Service:</strong> {serviceName}</p>
                            <p><strong>Date & Time:</strong> {serviceDate:MMMM dd, yyyy 'at' h:mm tt}</p>
                            <p><strong>Amount:</strong> ${amount:F2}</p>
                        </div>
                        
                        <p>Your service provider will contact you soon with any additional details.</p>
                        
                        <p>Thank you for choosing PetCare Platform!</p>
                        
                        <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                        <p style='color: #666; font-size: 12px; text-align: center;'>
                            This is an automated message from PetCare Platform
                        </p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendBookingCancellationAsync(string to, string petOwnerName, string serviceName, DateTime serviceDate)
        {
            var subject = "Booking Cancelled - PetCare Platform";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 10px;'>
                        <h2 style='color: #dc3545; text-align: center;'>❌ Booking Cancelled</h2>
                        
                        <p>Dear {petOwnerName},</p>
                        
                        <p>Your booking has been cancelled.</p>
                        
                        <div style='background-color: white; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                            <h3 style='color: #333; margin-top: 0;'>Cancelled Booking</h3>
                            <p><strong>Service:</strong> {serviceName}</p>
                            <p><strong>Date & Time:</strong> {serviceDate:MMMM dd, yyyy 'at' h:mm tt}</p>
                        </div>
                        
                        <p>If you have any questions about this cancellation, please contact our support team.</p>
                        
                        <p>We hope to serve you again soon!</p>
                        
                        <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                        <p style='color: #666; font-size: 12px; text-align: center;'>
                            This is an automated message from PetCare Platform
                        </p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPaymentConfirmationAsync(string to, string petOwnerName, string serviceName, decimal amount, int paymentId)
        {
            var subject = "Payment Confirmed - PetCare Platform";
            var receiptUrl = $"https://yourdomain.com/Payments/Receipt/{paymentId}";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 10px;'>
                        <h2 style='color: #28a745; text-align: center;'>💳 Payment Confirmed!</h2>
                        
                        <p>Dear {petOwnerName},</p>
                        
                        <p>Your payment has been successfully processed.</p>
                        
                        <div style='background-color: white; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                            <h3 style='color: #333; margin-top: 0;'>Payment Details</h3>
                            <p><strong>Service:</strong> {serviceName}</p>
                            <p><strong>Amount Paid:</strong> ${amount:F2}</p>
                            <p><strong>Payment Date:</strong> {DateTime.Now:MMMM dd, yyyy 'at' h:mm tt}</p>
                        </div>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{receiptUrl}' style='background-color: #007bff; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                                View Receipt
                            </a>
                        </div>
                        
                        <p>Your booking is now confirmed and your service provider has been notified.</p>
                        
                        <p>Thank you for your business!</p>
                        
                        <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                        <p style='color: #666; font-size: 12px; text-align: center;'>
                            This is an automated message from PetCare Platform
                        </p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendRefundConfirmationAsync(string to, string petOwnerName, string serviceName, decimal refundAmount, string reason)
        {
            var subject = "Refund Processed - PetCare Platform";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 10px;'>
                        <h2 style='color: #17a2b8; text-align: center;'>💰 Refund Processed</h2>
                        
                        <p>Dear {petOwnerName},</p>
                        
                        <p>Your refund has been successfully processed.</p>
                        
                        <div style='background-color: white; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                            <h3 style='color: #333; margin-top: 0;'>Refund Details</h3>
                            <p><strong>Service:</strong> {serviceName}</p>
                            <p><strong>Refund Amount:</strong> ${refundAmount:F2}</p>
                            <p><strong>Refund Date:</strong> {DateTime.Now:MMMM dd, yyyy 'at' h:mm tt}</p>
                            <p><strong>Reason:</strong> {reason ?? "Not specified"}</p>
                        </div>
                        
                        <div style='background-color: #fff3cd; padding: 15px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ffc107;'>
                            <p style='margin: 0;'><strong>Note:</strong> Refunds typically take 5-10 business days to appear in your account, depending on your bank or card issuer.</p>
                        </div>
                        
                        <p>If you have any questions about this refund, please contact our support team.</p>
                        
                        <p>Thank you for using PetCare Platform!</p>
                        
                        <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                        <p style='color: #666; font-size: 12px; text-align: center;'>
                            This is an automated message from PetCare Platform
                        </p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendReviewRequestAsync(string to, string petOwnerName, string serviceName, int bookingId)
        {
            var subject = "How was your service? - PetCare Platform";
            var reviewUrl = $"http://localhost:5090/Reviews/Create?bookingId={bookingId}";
            
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background-color: #f8f9fa; padding: 20px; border-radius: 10px;'>
                        <h2 style='color: #ffc107; text-align: center;'>⭐ Share Your Experience!</h2>
                        
                        <p>Dear {petOwnerName},</p>
                        
                        <p>We hope you and your pet had a great experience with {serviceName}!</p>
                        
                        <p>Your feedback is important to us and helps other pet owners make informed decisions.</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{reviewUrl}' style='background-color: #007bff; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                                Leave a Review
                            </a>
                        </div>
                        
                        <p>It only takes a minute and helps our community grow!</p>
                        
                        <p>Thank you for choosing PetCare Platform!</p>
                        
                        <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                        <p style='color: #666; font-size: 12px; text-align: center;'>
                            This is an automated message from PetCare Platform
                        </p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(to, subject, body);
        }
    }
}
