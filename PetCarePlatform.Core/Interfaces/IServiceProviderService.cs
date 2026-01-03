using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PetCarePlatform.Core.Common;
using PetCarePlatform.Core.DTOs.Requests;
using PetCarePlatform.Core.DTOs.Responses;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IServiceProviderService
    {
        Task<Result<ServiceProviderResponse>> GetServiceProviderByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<ServiceProviderResponse>> GetServiceProviderByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<Result<ServiceProviderResponse>> CreateServiceProviderProfileAsync(CreateServiceProviderRequest request, CancellationToken cancellationToken = default);
        Task<Result<ServiceProviderResponse>> UpdateServiceProviderProfileAsync(UpdateServiceProviderRequest request, CancellationToken cancellationToken = default);
        Task<Result<decimal>> GetProviderEarningsAsync(int providerId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<AvailabilitySchedule>>> GetAvailabilityScheduleAsync(int providerId, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<ReviewResponse>>> GetProviderReviewsAsync(int providerId, CancellationToken cancellationToken = default);
    }
}
