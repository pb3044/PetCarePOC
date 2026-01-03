using Microsoft.Extensions.Logging;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.DTOs.Responses;
using PaymentModel = PetCarePlatform.Core.Models.Payment;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PetCarePlatform.Infrastructure.Receipt
{
    public class ReceiptService : IReceiptService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<ReceiptService> _logger;

        public ReceiptService(
            IBookingRepository bookingRepository,
            ILogger<ReceiptService> logger)
        {
            _bookingRepository = bookingRepository;
            _logger = logger;
        }

        public async Task<string> GenerateReceiptAsync(PaymentModel payment)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(payment.BookingId);
                if (booking == null)
                {
                    throw new InvalidOperationException("Booking not found for payment");
                }

                var receiptNumber = $"RCP-{payment.Id:D6}-{payment.CreatedAt:yyyyMMdd}";
                var receiptDate = payment.CreatedAt.ToString("MMMM dd, yyyy 'at' h:mm tt");
                var serviceDate = booking.StartTime.ToString("MMMM dd, yyyy 'at' h:mm tt");

                var receiptHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Receipt - {receiptNumber}</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            max-width: 800px;
            margin: 0 auto;
            padding: 20px;
            color: #333;
        }}
        .receipt-header {{
            text-align: center;
            border-bottom: 3px solid #007bff;
            padding-bottom: 20px;
            margin-bottom: 30px;
        }}
        .receipt-header h1 {{
            color: #007bff;
            margin: 0;
            font-size: 28px;
        }}
        .receipt-info {{
            display: flex;
            justify-content: space-between;
            margin-bottom: 30px;
        }}
        .info-section {{
            flex: 1;
        }}
        .info-section h3 {{
            color: #007bff;
            border-bottom: 2px solid #007bff;
            padding-bottom: 5px;
            margin-bottom: 10px;
        }}
        .receipt-details {{
            background-color: #f8f9fa;
            padding: 20px;
            border-radius: 8px;
            margin-bottom: 30px;
        }}
        .detail-row {{
            display: flex;
            justify-content: space-between;
            padding: 10px 0;
            border-bottom: 1px solid #dee2e6;
        }}
        .detail-row:last-child {{
            border-bottom: none;
        }}
        .detail-label {{
            font-weight: bold;
            color: #666;
        }}
        .detail-value {{
            color: #333;
        }}
        .total-section {{
            background-color: #007bff;
            color: white;
            padding: 20px;
            border-radius: 8px;
            text-align: center;
        }}
        .total-section h2 {{
            margin: 0;
            font-size: 32px;
        }}
        .footer {{
            text-align: center;
            margin-top: 40px;
            padding-top: 20px;
            border-top: 1px solid #dee2e6;
            color: #666;
            font-size: 12px;
        }}
    </style>
</head>
<body>
    <div class='receipt-header'>
        <h1>🐾 PetCare Platform</h1>
        <h2>Payment Receipt</h2>
        <p>Receipt Number: <strong>{receiptNumber}</strong></p>
    </div>

    <div class='receipt-info'>
        <div class='info-section'>
            <h3>Payment Information</h3>
            <p><strong>Receipt Date:</strong> {receiptDate}</p>
            <p><strong>Transaction ID:</strong> {payment.TransactionId}</p>
            <p><strong>Payment Status:</strong> {payment.Status}</p>
            <p><strong>Payment Method:</strong> {payment.Method}</p>
        </div>
        <div class='info-section'>
            <h3>Service Details</h3>
            <p><strong>Service:</strong> {booking.Service?.Title ?? "N/A"}</p>
            <p><strong>Service Date:</strong> {serviceDate}</p>
            <p><strong>Pet:</strong> {booking.Pet?.Name ?? "N/A"}</p>
            <p><strong>Booking ID:</strong> #{booking.Id}</p>
        </div>
    </div>

    <div class='receipt-details'>
        <h3 style='color: #007bff; margin-top: 0;'>Payment Breakdown</h3>
        <div class='detail-row'>
            <span class='detail-label'>Service Amount:</span>
            <span class='detail-value'>${payment.Amount:F2}</span>
        </div>
        <div class='detail-row'>
            <span class='detail-label'>Platform Fee (15%):</span>
            <span class='detail-value'>-${payment.PlatformFee:F2}</span>
        </div>
        <div class='detail-row'>
            <span class='detail-label'>Provider Payout:</span>
            <span class='detail-value'>${payment.ProviderPayout:F2}</span>
        </div>
    </div>

    <div class='total-section'>
        <p style='margin: 0 0 10px 0; font-size: 16px;'>Total Amount Paid</p>
        <h2>${payment.Amount:F2} CAD</h2>
    </div>

    <div class='footer'>
        <p>Thank you for using PetCare Platform!</p>
        <p>This is an official receipt for your records.</p>
        <p>For questions or support, please contact us at support@petcareplatform.com</p>
    </div>
</body>
</html>";

                _logger.LogInformation("Receipt generated for payment {PaymentId}", payment.Id);
                return receiptHtml;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating receipt for payment {PaymentId}", payment.Id);
                throw;
            }
        }

        public async Task<byte[]> GenerateReceiptPdfAsync(PaymentModel payment)
        {
            // For MVP, we'll return the HTML as bytes
            // In production, you would use a library like QuestPDF or iTextSharp to generate actual PDFs
            var html = await GenerateReceiptAsync(payment);
            return Encoding.UTF8.GetBytes(html);
        }

        public async Task<string> GenerateReceiptFromPaymentResponseAsync(PaymentResponse payment)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(payment.BookingId);
                if (booking == null)
                {
                    throw new InvalidOperationException("Booking not found for payment");
                }

                var receiptNumber = $"RCP-{payment.Id:D6}-{payment.CreatedAt:yyyyMMdd}";
                var receiptDate = payment.CreatedAt.ToString("MMMM dd, yyyy 'at' h:mm tt");
                var serviceDate = booking.StartTime.ToString("MMMM dd, yyyy 'at' h:mm tt");

                var receiptHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Receipt - {receiptNumber}</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            max-width: 800px;
            margin: 0 auto;
            padding: 20px;
            color: #333;
        }}
        .receipt-header {{
            text-align: center;
            border-bottom: 3px solid #007bff;
            padding-bottom: 20px;
            margin-bottom: 30px;
        }}
        .receipt-header h1 {{
            color: #007bff;
            margin: 0;
            font-size: 28px;
        }}
        .receipt-info {{
            display: flex;
            justify-content: space-between;
            margin-bottom: 30px;
        }}
        .info-section {{
            flex: 1;
        }}
        .info-section h3 {{
            color: #007bff;
            border-bottom: 2px solid #007bff;
            padding-bottom: 5px;
            margin-bottom: 10px;
        }}
        .receipt-details {{
            background-color: #f8f9fa;
            padding: 20px;
            border-radius: 8px;
            margin-bottom: 30px;
        }}
        .detail-row {{
            display: flex;
            justify-content: space-between;
            padding: 10px 0;
            border-bottom: 1px solid #dee2e6;
        }}
        .detail-row:last-child {{
            border-bottom: none;
        }}
        .detail-label {{
            font-weight: bold;
            color: #666;
        }}
        .detail-value {{
            color: #333;
        }}
        .total-section {{
            background-color: #007bff;
            color: white;
            padding: 20px;
            border-radius: 8px;
            text-align: center;
        }}
        .total-section h2 {{
            margin: 0;
            font-size: 32px;
        }}
        .footer {{
            text-align: center;
            margin-top: 40px;
            padding-top: 20px;
            border-top: 1px solid #dee2e6;
            color: #666;
            font-size: 12px;
        }}
    </style>
</head>
<body>
    <div class='receipt-header'>
        <h1>🐾 PetCare Platform</h1>
        <h2>Payment Receipt</h2>
        <p>Receipt Number: <strong>{receiptNumber}</strong></p>
    </div>

    <div class='receipt-info'>
        <div class='info-section'>
            <h3>Payment Information</h3>
            <p><strong>Receipt Date:</strong> {receiptDate}</p>
            <p><strong>Transaction ID:</strong> {payment.TransactionId}</p>
            <p><strong>Payment Status:</strong> {payment.Status}</p>
            <p><strong>Payment Method:</strong> {payment.Method}</p>
        </div>
        <div class='info-section'>
            <h3>Service Details</h3>
            <p><strong>Service:</strong> {payment.BookingServiceName}</p>
            <p><strong>Booking ID:</strong> #{payment.BookingId}</p>
            <p><strong>Customer:</strong> {payment.UserName}</p>
        </div>
    </div>

    <div class='receipt-details'>
        <h3 style='color: #007bff; margin-top: 0;'>Payment Breakdown</h3>
        <div class='detail-row'>
            <span class='detail-label'>Service Amount:</span>
            <span class='detail-value'>${payment.Amount:F2}</span>
        </div>
        <div class='detail-row'>
            <span class='detail-label'>Platform Fee (15%):</span>
            <span class='detail-value'>-${payment.PlatformFee:F2}</span>
        </div>
        <div class='detail-row'>
            <span class='detail-label'>Provider Payout:</span>
            <span class='detail-value'>${payment.ProviderPayout:F2}</span>
        </div>
    </div>

    <div class='total-section'>
        <p style='margin: 0 0 10px 0; font-size: 16px;'>Total Amount Paid</p>
        <h2>${payment.Amount:F2} CAD</h2>
    </div>

    <div class='footer'>
        <p>Thank you for using PetCare Platform!</p>
        <p>This is an official receipt for your records.</p>
        <p>For questions or support, please contact us at support@petcareplatform.com</p>
    </div>
</body>
</html>";

                _logger.LogInformation("Receipt generated for payment {PaymentId}", payment.Id);
                return receiptHtml;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating receipt for payment {PaymentId}", payment.Id);
                throw;
            }
        }

        public async Task<byte[]> GenerateReceiptPdfFromPaymentResponseAsync(PaymentResponse payment)
        {
            // For MVP, we'll return the HTML as bytes
            // In production, you would use a library like QuestPDF or iTextSharp to generate actual PDFs
            var html = await GenerateReceiptFromPaymentResponseAsync(payment);
            return Encoding.UTF8.GetBytes(html);
        }
    }
}

