using System.Threading.Tasks;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Core.DTOs.Responses;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IReceiptService
    {
        Task<string> GenerateReceiptAsync(Payment payment);
        Task<byte[]> GenerateReceiptPdfAsync(Payment payment);
        Task<string> GenerateReceiptFromPaymentResponseAsync(PaymentResponse payment);
        Task<byte[]> GenerateReceiptPdfFromPaymentResponseAsync(PaymentResponse payment);
    }
}

