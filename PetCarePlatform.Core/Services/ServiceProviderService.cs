using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PetCarePlatform.Core.Common;
using PetCarePlatform.Core.DTOs.Requests;
using PetCarePlatform.Core.DTOs.Responses;
using PetCarePlatform.Core.Exceptions;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Services
{
    public class ServiceProviderService : IServiceProviderService
    {
        private readonly IServiceProviderRepository _serviceProviderRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ServiceProviderService> _logger;
        
        public ServiceProviderService(
            IServiceProviderRepository serviceProviderRepository,
            IServiceRepository serviceRepository,
            IBookingRepository bookingRepository,
            IPaymentRepository paymentRepository,
            IUserRepository userRepository,
            ILogger<ServiceProviderService> logger)
        {
            _serviceProviderRepository = serviceProviderRepository ?? throw new ArgumentNullException(nameof(serviceProviderRepository));
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ServiceProvider> GetServiceProviderByIdAsync(int id)
        {
            return await _serviceProviderRepository.GetByIdAsync(id);
        }

        public async Task<ServiceProvider> GetServiceProviderByUserIdAsync(int userId)
        {
            return await _serviceProviderRepository.GetByUserIdAsync(userId);
        }

        public async Task<ServiceProvider> CreateServiceProviderProfileAsync(ServiceProvider serviceProvider)
        {
            // Set default values
            serviceProvider.CreatedAt = DateTime.UtcNow;
            serviceProvider.UpdatedAt = DateTime.UtcNow;
            serviceProvider.AverageRating = 0;
            serviceProvider.TotalReviews = 0;
            serviceProvider.BackgroundCheckVerified = false;
            serviceProvider.IdentityVerified = false;

            return await _serviceProviderRepository.CreateAsync(serviceProvider);
        }

        public async Task UpdateServiceProviderProfileAsync(ServiceProvider serviceProvider)
        {
            var existingProvider = await _serviceProviderRepository.GetByIdAsync(serviceProvider.Id);
            if (existingProvider == null)
            {
                throw new InvalidOperationException("Service provider profile not found");
            }

            // Update fields
            existingProvider.BusinessName = serviceProvider.BusinessName;
            existingProvider.Description = serviceProvider.Description;
            existingProvider.Credentials = serviceProvider.Credentials;
            existingProvider.Certifications = serviceProvider.Certifications;
            existingProvider.InsuranceInfo = serviceProvider.InsuranceInfo;
            existingProvider.LicenseInfo = serviceProvider.LicenseInfo;
            existingProvider.ServiceArea = serviceProvider.ServiceArea;
            existingProvider.ServiceRadius = serviceProvider.ServiceRadius;
            existingProvider.BankingInfo = serviceProvider.BankingInfo;
            existingProvider.TaxInfo = serviceProvider.TaxInfo;
            existingProvider.UpdatedAt = DateTime.UtcNow;

            await _serviceProviderRepository.UpdateAsync(existingProvider);
        }

        public async Task<IEnumerable<Service>> GetServicesByProviderIdAsync(int providerId)
        {
            return await _serviceRepository.GetByProviderIdAsync(providerId);
        }

        public async Task<Service> AddServiceAsync(Service service)
        {
            // Set default values
            service.CreatedAt = DateTime.UtcNow;
            service.UpdatedAt = DateTime.UtcNow;
            service.IsActive = true;

            return await _serviceRepository.CreateAsync(service);
        }

        public async Task UpdateServiceAsync(Service service)
        {
            var existingService = await _serviceRepository.GetByIdAsync(service.Id);
            if (existingService == null)
            {
                throw new InvalidOperationException("Service not found");
            }

            // Update fields
            existingService.Title = service.Title;
            existingService.Description = service.Description;
            existingService.Type = service.Type;
            existingService.BasePrice = service.BasePrice;
            existingService.PriceUnit = service.PriceUnit;
            existingService.IsActive = service.IsActive;
            existingService.Location = service.Location;
            existingService.Latitude = service.Latitude;
            existingService.Longitude = service.Longitude;
            existingService.AcceptedPetTypes = service.AcceptedPetTypes;
            existingService.AcceptedPetSizes = service.AcceptedPetSizes;
            existingService.MaxPetsPerBooking = service.MaxPetsPerBooking;
            existingService.UpdatedAt = DateTime.UtcNow;

            await _serviceRepository.UpdateAsync(existingService);
        }

        public async Task DeleteServiceAsync(int serviceId)
        {
            await _serviceRepository.DeleteAsync(serviceId);
        }

        public async Task<IEnumerable<Booking>> GetProviderBookingsAsync(int providerId, bool includeHistory = false)
        {
            // This would typically filter by status based on includeHistory parameter
            return await _bookingRepository.GetByProviderIdAsync(providerId);
        }

        public async Task<bool> UpdateAvailabilityScheduleAsync(int providerId, IEnumerable<AvailabilitySchedule> schedules)
        {
            var provider = await _serviceProviderRepository.GetByIdAsync(providerId);
            if (provider == null)
            {
                throw new InvalidOperationException("Service provider not found");
            }

            // In a real implementation, we would update the availability schedules in the database
            // For now, we'll just return true
            return true;
        }


        public async Task<decimal> GetProviderEarningsAsync(int providerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            return await _paymentRepository.GetProviderEarningsAsync(providerId);
        }

        // ============================================
        // Enterprise Pattern Methods (Result-based)
        // ============================================

        public async Task<Result<ServiceProviderResponse>> GetServiceProviderByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting service provider by ID: {ProviderId}", id);
                
                var provider = await _serviceProviderRepository.GetByIdAsync(id).ConfigureAwait(false);
                if (provider == null)
                {
                    _logger.LogWarning("Service provider not found: {ProviderId}", id);
                    return Result<ServiceProviderResponse>.Failure("Service provider not found", "PROVIDER_NOT_FOUND");
                }

                var response = await MapToServiceProviderResponseAsync(provider).ConfigureAwait(false);
                return Result<ServiceProviderResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service provider {ProviderId}", id);
                return Result<ServiceProviderResponse>.Failure("An error occurred while retrieving the service provider", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<ServiceProviderResponse>> GetServiceProviderByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting service provider by user ID: {UserId}", userId);
                
                var provider = await _serviceProviderRepository.GetByUserIdAsync(userId).ConfigureAwait(false);
                if (provider == null)
                {
                    _logger.LogWarning("Service provider not found for user: {UserId}", userId);
                    return Result<ServiceProviderResponse>.Failure("Service provider not found", "PROVIDER_NOT_FOUND");
                }

                var response = await MapToServiceProviderResponseAsync(provider).ConfigureAwait(false);
                return Result<ServiceProviderResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service provider for user {UserId}", userId);
                return Result<ServiceProviderResponse>.Failure("An error occurred while retrieving the service provider", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<ServiceProviderResponse>> CreateServiceProviderProfileAsync(CreateServiceProviderRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating service provider profile for user {UserId}", request.UserId);

                // Validate user exists
                var user = await _userRepository.GetByIdAsync(request.UserId).ConfigureAwait(false);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", request.UserId);
                    return Result<ServiceProviderResponse>.Failure("User not found", "USER_NOT_FOUND");
                }

                // Check if provider profile already exists
                var existingProvider = await _serviceProviderRepository.GetByUserIdAsync(request.UserId).ConfigureAwait(false);
                if (existingProvider != null)
                {
                    _logger.LogWarning("Service provider profile already exists for user: {UserId}", request.UserId);
                    return Result<ServiceProviderResponse>.Failure(
                        "Service provider profile already exists for this user", 
                        "PROVIDER_ALREADY_EXISTS");
                }

                // Create service provider
                var serviceProvider = new ServiceProvider
                {
                    UserId = request.UserId,
                    BusinessName = request.BusinessName,
                    BusinessType = request.BusinessType,
                    BusinessNumber = request.BusinessNumber,
                    Description = request.Description,
                    Credentials = request.Credentials,
                    Certifications = request.Certifications,
                    InsuranceInfo = request.InsuranceInfo,
                    LicenseInfo = request.LicenseInfo,
                    ServiceArea = request.ServiceArea,
                    ServiceRadius = request.ServiceRadius,
                    AverageRating = 0,
                    TotalReviews = 0,
                    BackgroundCheckVerified = false,
                    IdentityVerified = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdProvider = await _serviceProviderRepository.CreateAsync(serviceProvider).ConfigureAwait(false);

                _logger.LogInformation("Service provider profile created successfully: {ProviderId}", createdProvider.Id);

                var response = await MapToServiceProviderResponseAsync(createdProvider).ConfigureAwait(false);
                return Result<ServiceProviderResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service provider profile for user {UserId}", request.UserId);
                return Result<ServiceProviderResponse>.Failure("An error occurred while creating the service provider profile", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<ServiceProviderResponse>> UpdateServiceProviderProfileAsync(UpdateServiceProviderRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating service provider profile {ProviderId}", request.ProviderId);

                var existingProvider = await _serviceProviderRepository.GetByIdAsync(request.ProviderId).ConfigureAwait(false);
                if (existingProvider == null)
                {
                    _logger.LogWarning("Service provider not found: {ProviderId}", request.ProviderId);
                    return Result<ServiceProviderResponse>.Failure("Service provider not found", "PROVIDER_NOT_FOUND");
                }

                // Update fields
                existingProvider.BusinessName = request.BusinessName;
                existingProvider.Description = request.Description;
                existingProvider.Credentials = request.Credentials;
                existingProvider.Certifications = request.Certifications;
                existingProvider.InsuranceInfo = request.InsuranceInfo;
                existingProvider.LicenseInfo = request.LicenseInfo;
                existingProvider.ServiceArea = request.ServiceArea;
                if (request.ServiceRadius.HasValue)
                {
                    existingProvider.ServiceRadius = request.ServiceRadius.Value;
                }
                existingProvider.UpdatedAt = DateTime.UtcNow;

                await _serviceProviderRepository.UpdateAsync(existingProvider).ConfigureAwait(false);

                _logger.LogInformation("Service provider profile updated successfully: {ProviderId}", request.ProviderId);

                var response = await MapToServiceProviderResponseAsync(existingProvider).ConfigureAwait(false);
                return Result<ServiceProviderResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service provider profile {ProviderId}", request.ProviderId);
                return Result<ServiceProviderResponse>.Failure("An error occurred while updating the service provider profile", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<decimal>> GetProviderEarningsAsync(int providerId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting provider earnings for provider {ProviderId}", providerId);

                var earnings = await _paymentRepository.GetProviderEarningsAsync(providerId).ConfigureAwait(false);
                return Result<decimal>.Success(earnings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provider earnings for provider {ProviderId}", providerId);
                return Result<decimal>.Failure("An error occurred while retrieving provider earnings", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<IEnumerable<AvailabilitySchedule>>> GetAvailabilityScheduleAsync(int providerId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting availability schedule for provider {ProviderId}", providerId);

                var provider = await _serviceProviderRepository.GetByIdAsync(providerId).ConfigureAwait(false);
                if (provider == null)
                {
                    _logger.LogWarning("Service provider not found: {ProviderId}", providerId);
                    return Result<IEnumerable<AvailabilitySchedule>>.Failure("Service provider not found", "PROVIDER_NOT_FOUND");
                }

                var schedules = provider.AvailabilitySchedules ?? Enumerable.Empty<AvailabilitySchedule>();
                return Result<IEnumerable<AvailabilitySchedule>>.Success(schedules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting availability schedule for provider {ProviderId}", providerId);
                return Result<IEnumerable<AvailabilitySchedule>>.Failure("An error occurred while retrieving availability schedule", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<ServiceResponse>> AddServiceAsync(CreateServiceRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Adding service for provider {ProviderId}", request.ProviderId);

                // Validate provider exists
                var provider = await _serviceProviderRepository.GetByIdAsync(request.ProviderId).ConfigureAwait(false);
                if (provider == null)
                {
                    _logger.LogWarning("Service provider not found: {ProviderId}", request.ProviderId);
                    return Result<ServiceResponse>.Failure("Service provider not found", "PROVIDER_NOT_FOUND");
                }

                // Create service using repository
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

                // Map to response
                var rating = await _serviceRepository.GetAverageRatingAsync(createdService.Id).ConfigureAwait(false);
                var response = new ServiceResponse
                {
                    Id = createdService.Id,
                    ProviderId = createdService.ProviderId,
                    ProviderName = $"{provider.User?.FirstName} {provider.User?.LastName}".Trim(),
                    ProviderBusinessName = provider.BusinessName,
                    Title = createdService.Title,
                    Description = createdService.Description,
                    Type = createdService.Type,
                    BasePrice = createdService.BasePrice,
                    PriceUnit = createdService.PriceUnit,
                    IsActive = createdService.IsActive,
                    Location = createdService.Location,
                    Latitude = createdService.Latitude,
                    Longitude = createdService.Longitude,
                    AcceptedPetTypes = createdService.AcceptedPetTypes,
                    AcceptedPetSizes = createdService.AcceptedPetSizes,
                    MaxPetsPerBooking = createdService.MaxPetsPerBooking,
                    AverageRating = rating,
                    ReviewCount = 0,
                    CreatedAt = createdService.CreatedAt,
                    UpdatedAt = createdService.UpdatedAt,
                    PhotoUrls = new List<string>()
                };

                return Result<ServiceResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding service for provider {ProviderId}", request.ProviderId);
                return Result<ServiceResponse>.Failure("An error occurred while creating the service", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<IEnumerable<ReviewResponse>>> GetProviderReviewsAsync(int providerId, CancellationToken cancellationToken = default)
        {
            // This method needs IReviewService to get reviews
            // For now, return empty list - the controller should use IReviewService directly
            try
            {
                _logger.LogInformation("Getting reviews for provider {ProviderId}", providerId);
                return Result<IEnumerable<ReviewResponse>>.Success(Enumerable.Empty<ReviewResponse>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews for provider {ProviderId}", providerId);
                return Result<IEnumerable<ReviewResponse>>.Failure("An error occurred while retrieving reviews", "INTERNAL_ERROR");
            }
        }

        // Helper method to map ServiceProvider to ServiceProviderResponse
        private async Task<ServiceProviderResponse> MapToServiceProviderResponseAsync(ServiceProvider provider)
        {
            var services = await _serviceRepository.GetByProviderIdAsync(provider.Id).ConfigureAwait(false);
            var serviceCount = services?.Count() ?? 0;

            return new ServiceProviderResponse
            {
                Id = provider.Id,
                UserId = provider.UserId,
                UserName = $"{provider.User?.FirstName} {provider.User?.LastName}".Trim(),
                UserEmail = provider.User?.Email ?? string.Empty,
                BusinessName = provider.BusinessName,
                BusinessType = provider.BusinessType,
                BusinessNumber = provider.BusinessNumber,
                Description = provider.Description,
                Credentials = provider.Credentials,
                Certifications = provider.Certifications,
                InsuranceInfo = provider.InsuranceInfo,
                LicenseInfo = provider.LicenseInfo,
                BackgroundCheckVerified = provider.BackgroundCheckVerified,
                IdentityVerified = provider.IdentityVerified,
                AverageRating = provider.AverageRating,
                TotalReviews = provider.TotalReviews,
                ServiceArea = provider.ServiceArea,
                ServiceRadius = provider.ServiceRadius,
                IsActive = provider.IsActive,
                CreatedAt = provider.CreatedAt,
                UpdatedAt = provider.UpdatedAt,
                ServiceCount = serviceCount
            };
        }
    }
}
