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
    public interface IServiceService
    {
        Task<Result<ServiceResponse>> GetServiceByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<PagedResult<ServiceResponse>>> GetServicesAsync(ServiceQuery query, CancellationToken cancellationToken = default);
        Task<Result<ServiceResponse>> CreateServiceAsync(CreateServiceRequest request, CancellationToken cancellationToken = default);
        Task<Result<ServiceResponse>> UpdateServiceAsync(UpdateServiceRequest request, CancellationToken cancellationToken = default);
        Task<Result> DeleteServiceAsync(int serviceId, CancellationToken cancellationToken = default);
        Task<Result<double>> GetServiceRatingAsync(int serviceId, CancellationToken cancellationToken = default);
        Task<Result<PagedResult<ServiceResponse>>> SearchServicesAsync(ServiceQuery query, CancellationToken cancellationToken = default);
        Task<Result<PagedResult<ReviewResponse>>> GetServiceReviewsAsync(int serviceId, CancellationToken cancellationToken = default);
        Task<Result<List<ServicePhotoResponse>>> GetServicePhotosAsync(int serviceId, CancellationToken cancellationToken = default);
        Task<Result<IEnumerable<ServiceResponse>>> GetServicesByProviderIdAsync(int providerId, CancellationToken cancellationToken = default);
    }
}
