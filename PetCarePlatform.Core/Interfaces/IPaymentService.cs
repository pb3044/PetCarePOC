using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PetCarePlatform.Core.Common;
using PetCarePlatform.Core.DTOs.Queries;
using PetCarePlatform.Core.DTOs.Requests;
using PetCarePlatform.Core.DTOs.Responses;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IPaymentService
    {
        Task<Result<PaymentResponse>> GetPaymentByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<PaymentResponse>> GetPaymentByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default);
        Task<Result<PaymentResponse>> GetPaymentByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default);
        Task<Result<PagedResult<PaymentResponse>>> GetPaymentsAsync(PaymentQuery query, CancellationToken cancellationToken = default);
        Task<Result<PaymentResponse>> CreatePaymentIntentAsync(CreatePaymentIntentRequest request, CancellationToken cancellationToken = default);
        Task<Result<PaymentResponse>> ConfirmPaymentAsync(ConfirmPaymentRequest request, CancellationToken cancellationToken = default);
        Task<Result<PaymentResponse>> ProcessRefundAsync(ProcessRefundRequest request, CancellationToken cancellationToken = default);
        Task<Result<decimal>> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
        Task<Result<decimal>> GetProviderEarningsAsync(int providerId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
        Task<Result<decimal>> CalculatePlatformFeeAsync(decimal amount, CancellationToken cancellationToken = default);
        Task<Result> UpdatePaymentStatusAsync(int paymentId, PaymentStatus status, CancellationToken cancellationToken = default);
    }
}
