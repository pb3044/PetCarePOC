using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PetCarePlatform.Core.Common;
using PetCarePlatform.Core.DTOs.Queries;
using PetCarePlatform.Core.DTOs.Requests;
using PetCarePlatform.Core.DTOs.Responses;
using PetCarePlatform.Core.Exceptions;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly ILogger<ServiceService> _logger;
        
        public ServiceService(
            IServiceRepository serviceRepository, 
            IReviewRepository reviewRepository,
            ILogger<ServiceService> logger)
        {
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ============================================
        // Enterprise Pattern Methods (Result-based)
        // ============================================

        public async Task<Result<ServiceResponse>> GetServiceByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting service by ID: {ServiceId}", id);
                
                var service = await _serviceRepository.GetByIdAsync(id).ConfigureAwait(false);
                if (service == null)
                {
                    _logger.LogWarning("Service not found: {ServiceId}", id);
                    return Result<ServiceResponse>.Failure("Service not found", "SERVICE_NOT_FOUND");
                }

                var response = await MapToServiceResponseAsync(service).ConfigureAwait(false);
                return Result<ServiceResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service {ServiceId}", id);
                return Result<ServiceResponse>.Failure("An error occurred while retrieving the service", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<PagedResult<ServiceResponse>>> GetServicesAsync(ServiceQuery query, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting services with query: ProviderId={ProviderId}, Type={Type}, Keyword={Keyword}", 
                    query.ProviderId, query.Type, query.Keyword);

                IEnumerable<Service> services;

                if (query.ProviderId.HasValue)
                {
                    services = await _serviceRepository.GetByProviderIdAsync(query.ProviderId.Value).ConfigureAwait(false);
                }
                else
                {
                    // Use search method with query parameters
                    services = await _serviceRepository.SearchAsync(
                        query.Keyword,
                        query.Type,
                        query.Latitude,
                        query.Longitude,
                        query.RadiusInKm,
                        query.MinPrice,
                        query.MaxPrice,
                        query.PetTypes,
                        query.PetSizes
                    ).ConfigureAwait(false);
                }

                // Apply additional filters
                if (query.MinRating.HasValue)
                {
                    services = services.Where(s => s.Provider?.AverageRating >= query.MinRating.Value);
                }

                if (query.ShowVerifiedOnly == true)
                {
                    services = services.Where(s => s.Provider?.IsVerified == true);
                }

                // Apply sorting
                services = query.SortBy?.ToLower() switch
                {
                    "rating" => services.OrderByDescending(s => s.Provider?.AverageRating ?? 0),
                    "price_low" => services.OrderBy(s => s.BasePrice),
                    "price_high" => services.OrderByDescending(s => s.BasePrice),
                    "newest" => services.OrderByDescending(s => s.CreatedAt),
                    "distance" => services, // Distance sorting handled in repository
                    _ => services
                };

                var totalCount = services.Count();
                var items = services
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToList();

                var serviceResponses = new List<ServiceResponse>();
                foreach (var service in items)
                {
                    var response = await MapToServiceResponseAsync(service).ConfigureAwait(false);
                    serviceResponses.Add(response);
                }

                var pagedResult = new PagedResult<ServiceResponse>(
                    serviceResponses,
                    totalCount,
                    query.PageNumber,
                    query.PageSize
                );

                return Result<PagedResult<ServiceResponse>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting services");
                return Result<PagedResult<ServiceResponse>>.Failure("An error occurred while retrieving services", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<ServiceResponse>> CreateServiceAsync(CreateServiceRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating service for provider {ProviderId}", request.ProviderId);

                // Create service
                var service = new Service
                {
                    ProviderId = request.ProviderId,
                    Title = request.Title,
                    Description = request.Description,
                    Type = request.Type,
                    BasePrice = request.BasePrice,
                    PriceUnit = request.PriceUnit,
                    Location = request.Location ?? string.Empty,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    AcceptedPetTypes = request.AcceptedPetTypes,
                    AcceptedPetSizes = request.AcceptedPetSizes,
                    MaxPetsPerBooking = request.MaxPetsPerBooking ?? 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdService = await _serviceRepository.CreateAsync(service).ConfigureAwait(false);

                _logger.LogInformation("Service created successfully: {ServiceId}", createdService.Id);

                var response = await MapToServiceResponseAsync(createdService).ConfigureAwait(false);
                return Result<ServiceResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service for provider {ProviderId}", request.ProviderId);
                return Result<ServiceResponse>.Failure("An error occurred while creating the service", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<ServiceResponse>> UpdateServiceAsync(UpdateServiceRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating service {ServiceId}", request.ServiceId);

                var existingService = await _serviceRepository.GetByIdAsync(request.ServiceId).ConfigureAwait(false);
                if (existingService == null)
                {
                    _logger.LogWarning("Service not found: {ServiceId}", request.ServiceId);
                    return Result<ServiceResponse>.Failure("Service not found", "SERVICE_NOT_FOUND");
                }

                // Update fields
                existingService.Title = request.Title;
                existingService.Description = request.Description;
                existingService.Type = request.Type;
                existingService.BasePrice = request.BasePrice;
                existingService.PriceUnit = request.PriceUnit;
                existingService.IsActive = request.IsActive;
                existingService.Location = request.Location ?? string.Empty;
                existingService.Latitude = request.Latitude;
                existingService.Longitude = request.Longitude;
                existingService.AcceptedPetTypes = request.AcceptedPetTypes;
                existingService.AcceptedPetSizes = request.AcceptedPetSizes;
                existingService.MaxPetsPerBooking = request.MaxPetsPerBooking ?? existingService.MaxPetsPerBooking;
                existingService.UpdatedAt = DateTime.UtcNow;

                await _serviceRepository.UpdateAsync(existingService).ConfigureAwait(false);

                _logger.LogInformation("Service updated successfully: {ServiceId}", request.ServiceId);

                var response = await MapToServiceResponseAsync(existingService).ConfigureAwait(false);
                return Result<ServiceResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service {ServiceId}", request.ServiceId);
                return Result<ServiceResponse>.Failure("An error occurred while updating the service", "INTERNAL_ERROR");
            }
        }

        public async Task<Result> DeleteServiceAsync(int serviceId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Deleting service {ServiceId}", serviceId);

                var service = await _serviceRepository.GetByIdAsync(serviceId).ConfigureAwait(false);
                if (service == null)
                {
                    _logger.LogWarning("Service not found: {ServiceId}", serviceId);
                    return Result.Failure("Service not found", "SERVICE_NOT_FOUND");
                }

                await _serviceRepository.DeleteAsync(serviceId).ConfigureAwait(false);

                _logger.LogInformation("Service deleted successfully: {ServiceId}", serviceId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service {ServiceId}", serviceId);
                return Result.Failure("An error occurred while deleting the service", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<double>> GetServiceRatingAsync(int serviceId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting service rating for service {ServiceId}", serviceId);

                var rating = await _serviceRepository.GetAverageRatingAsync(serviceId).ConfigureAwait(false);
                return Result<double>.Success(rating);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service rating for service {ServiceId}", serviceId);
                return Result<double>.Failure("An error occurred while retrieving the service rating", "INTERNAL_ERROR");
            }
        }

        // Helper method to map Service to ServiceResponse
        private async Task<ServiceResponse> MapToServiceResponseAsync(Service service)
        {
            var rating = await _serviceRepository.GetAverageRatingAsync(service.Id).ConfigureAwait(false);
            var reviews = await _reviewRepository.GetByServiceIdAsync(service.Id).ConfigureAwait(false);
            var reviewCount = reviews?.Count() ?? 0;

            var photoUrls = service.Photos?.Select(p => p.Url).ToList() ?? new List<string>();

            return new ServiceResponse
            {
                Id = service.Id,
                ProviderId = service.ProviderId,
                ProviderName = $"{service.Provider?.User?.FirstName} {service.Provider?.User?.LastName}".Trim(),
                ProviderBusinessName = service.Provider?.BusinessName ?? string.Empty,
                Title = service.Title,
                Description = service.Description,
                Type = service.Type,
                BasePrice = service.BasePrice,
                PriceUnit = service.PriceUnit,
                IsActive = service.IsActive,
                Location = service.Location,
                Latitude = service.Latitude,
                Longitude = service.Longitude,
                AcceptedPetTypes = service.AcceptedPetTypes,
                AcceptedPetSizes = service.AcceptedPetSizes,
                MaxPetsPerBooking = service.MaxPetsPerBooking,
                AverageRating = rating,
                ReviewCount = reviewCount,
                CreatedAt = service.CreatedAt,
                UpdatedAt = service.UpdatedAt,
                PhotoUrls = photoUrls
            };
        }

        public async Task<Result<PagedResult<ServiceResponse>>> SearchServicesAsync(ServiceQuery query, CancellationToken cancellationToken = default)
        {
            // SearchServicesAsync is essentially the same as GetServicesAsync
            return await GetServicesAsync(query, cancellationToken);
        }

        public async Task<Result<PagedResult<ReviewResponse>>> GetServiceReviewsAsync(int serviceId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting reviews for service: {ServiceId}", serviceId);
                
                // This would need to be implemented using IReviewService
                // For now, return an empty result
                return Result<PagedResult<ReviewResponse>>.Failure("Method not yet implemented", "NOT_IMPLEMENTED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service reviews: {ServiceId}", serviceId);
                return Result<PagedResult<ReviewResponse>>.Failure("An error occurred while retrieving service reviews", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<List<ServicePhotoResponse>>> GetServicePhotosAsync(int serviceId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting photos for service: {ServiceId}", serviceId);
                
                var service = await _serviceRepository.GetByIdAsync(serviceId).ConfigureAwait(false);
                if (service == null)
                {
                    _logger.LogWarning("Service not found: {ServiceId}", serviceId);
                    return Result<List<ServicePhotoResponse>>.Failure("Service not found", "SERVICE_NOT_FOUND");
                }

                var photos = service.Photos?.Select(p => new ServicePhotoResponse
                {
                    Id = p.Id,
                    ServiceId = p.ServiceId,
                    Url = p.Url,
                    Caption = p.Caption,
                    IsPrimary = p.IsPrimary,
                    CreatedAt = p.CreatedAt
                }).ToList() ?? new List<ServicePhotoResponse>();

                return Result<List<ServicePhotoResponse>>.Success(photos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service photos: {ServiceId}", serviceId);
                return Result<List<ServicePhotoResponse>>.Failure("An error occurred while retrieving service photos", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<IEnumerable<ServiceResponse>>> GetServicesByProviderIdAsync(int providerId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting services for provider {ProviderId}", providerId);

                var services = await _serviceRepository.GetByProviderIdAsync(providerId).ConfigureAwait(false);
                var serviceResponses = new List<ServiceResponse>();
                
                foreach (var service in services)
                {
                    var response = await MapToServiceResponseAsync(service).ConfigureAwait(false);
                    serviceResponses.Add(response);
                }

                return Result<IEnumerable<ServiceResponse>>.Success(serviceResponses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting services for provider {ProviderId}", providerId);
                return Result<IEnumerable<ServiceResponse>>.Failure("An error occurred while retrieving services", "INTERNAL_ERROR");
            }
        }
    }
}
