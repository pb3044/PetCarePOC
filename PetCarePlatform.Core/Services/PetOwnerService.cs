using System;
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
    public class PetOwnerService : IPetOwnerService
    {
        private readonly IPetOwnerRepository _petOwnerRepository;
        private readonly IPetRepository _petRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<PetOwnerService> _logger;

        public PetOwnerService(
            IPetOwnerRepository petOwnerRepository, 
            IPetRepository petRepository,
            IUserRepository userRepository,
            IBookingRepository bookingRepository,
            ILogger<PetOwnerService> logger)
        {
            _petOwnerRepository = petOwnerRepository ?? throw new ArgumentNullException(nameof(petOwnerRepository));
            _petRepository = petRepository ?? throw new ArgumentNullException(nameof(petRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _bookingRepository = bookingRepository ?? throw new ArgumentNullException(nameof(bookingRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PetOwner> GetPetOwnerByIdAsync(int id)
        {
            return await _petOwnerRepository.GetByIdAsync(id);
        }

        public async Task<PetOwner> GetPetOwnerByUserIdAsync(int userId)
        {
            return await _petOwnerRepository.GetByUserIdAsync(userId);
        }

        public async Task<PetOwner> CreatePetOwnerProfileAsync(PetOwner petOwner)
        {
            // Set default values
            petOwner.CreatedAt = DateTime.UtcNow;
            petOwner.UpdatedAt = DateTime.UtcNow;
            petOwner.ReceiveMarketingEmails = petOwner.ReceiveMarketingEmails; // Use provided value
            petOwner.ReceiveNotifications = true; // Default to true for notifications

            return await _petOwnerRepository.CreateAsync(petOwner);
        }

        public async Task UpdatePetOwnerProfileAsync(PetOwner petOwner)
        {
            if (petOwner == null)
            {
                throw new ArgumentNullException(nameof(petOwner));
            }

            var existingPetOwner = await _petOwnerRepository.GetByIdAsync(petOwner.Id);
            if (existingPetOwner == null)
            {
                _logger.LogWarning("Pet owner profile not found: {PetOwnerId}", petOwner.Id);
                throw new EntityNotFoundException("PetOwner", petOwner.Id);
            }

            // Update fields
            existingPetOwner.PreferredServiceTypes = petOwner.PreferredServiceTypes;
            existingPetOwner.PreferredProviderAttributes = petOwner.PreferredProviderAttributes;
            existingPetOwner.ReceiveMarketingEmails = petOwner.ReceiveMarketingEmails;
            existingPetOwner.ReceiveNotifications = petOwner.ReceiveNotifications;
            existingPetOwner.UpdatedAt = DateTime.UtcNow;

            await _petOwnerRepository.UpdateAsync(existingPetOwner);
        }

        public async Task<IEnumerable<Pet>> GetPetsByOwnerIdAsync(int ownerId)
        {
            return await _petRepository.GetByOwnerIdAsync(ownerId);
        }

        public async Task<Pet> AddPetAsync(Pet pet)
        {
            // Set default values
            pet.CreatedAt = DateTime.UtcNow;
            pet.UpdatedAt = DateTime.UtcNow;

            return await _petRepository.CreateAsync(pet);
        }

        public async Task UpdatePetAsync(Pet pet)
        {
            if (pet == null)
            {
                throw new ArgumentNullException(nameof(pet));
            }

            var existingPet = await _petRepository.GetByIdAsync(pet.Id);
            if (existingPet == null)
            {
                _logger.LogWarning("Pet not found: {PetId}", pet.Id);
                throw new EntityNotFoundException("Pet", pet.Id);
            }

            // Update fields
            existingPet.Name = pet.Name;
            existingPet.Type = pet.Type;
            existingPet.Breed = pet.Breed;
            existingPet.Age = pet.Age;
            existingPet.Size = pet.Size;
            existingPet.Gender = pet.Gender;
            existingPet.IsNeutered = pet.IsNeutered;
            existingPet.MedicalInformation = pet.MedicalInformation;
            existingPet.SpecialNeeds = pet.SpecialNeeds;
            existingPet.Temperament = pet.Temperament;
            existingPet.FeedingInstructions = pet.FeedingInstructions;
            existingPet.ExerciseNeeds = pet.ExerciseNeeds;
            existingPet.BehavioralNotes = pet.BehavioralNotes;
            existingPet.EmergencyContactName = pet.EmergencyContactName;
            existingPet.EmergencyContactPhone = pet.EmergencyContactPhone;
            existingPet.VeterinarianName = pet.VeterinarianName;
            existingPet.VeterinarianPhone = pet.VeterinarianPhone;
            existingPet.UpdatedAt = DateTime.UtcNow;

            await _petRepository.UpdateAsync(existingPet);
        }

        public async Task DeletePetAsync(int petId)
        {
            await _petRepository.DeleteAsync(petId);
        }

        public async Task<IEnumerable<ServiceProvider>> GetFavoriteProvidersAsync(int petOwnerId)
        {
            return await _petOwnerRepository.GetFavoriteProvidersAsync(petOwnerId);
        }

        public async Task AddFavoriteProviderAsync(int petOwnerId, int providerId)
        {
            await _petOwnerRepository.AddFavoriteProviderAsync(petOwnerId, providerId);
        }

        public async Task RemoveFavoriteProviderAsync(int petOwnerId, int providerId)
        {
            await _petOwnerRepository.RemoveFavoriteProviderAsync(petOwnerId, providerId);
        }

        public async Task<IEnumerable<Booking>> GetOwnerBookingsAsync(int ownerId, bool includeHistory = false)
        {
            var petOwner = await _petOwnerRepository.GetByIdAsync(ownerId);
            if (petOwner == null)
            {
                return new List<Booking>();
            }
            
            // This would typically filter by status based on includeHistory parameter
            // For now, we'll just return all bookings for the owner
            return petOwner.Bookings ?? new List<Booking>();
        }

        // ============================================
        // Enterprise Pattern Methods (Result-based)
        // ============================================

        public async Task<Result<PetOwnerResponse>> GetPetOwnerByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting pet owner by ID: {PetOwnerId}", id);
                
                var petOwner = await _petOwnerRepository.GetByIdAsync(id).ConfigureAwait(false);
                if (petOwner == null)
                {
                    _logger.LogWarning("Pet owner not found: {PetOwnerId}", id);
                    return Result<PetOwnerResponse>.Failure("Pet owner not found", "PET_OWNER_NOT_FOUND");
                }

                var response = await MapToPetOwnerResponseAsync(petOwner).ConfigureAwait(false);
                return Result<PetOwnerResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pet owner {PetOwnerId}", id);
                return Result<PetOwnerResponse>.Failure("An error occurred while retrieving the pet owner", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<PetOwnerResponse>> GetPetOwnerByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting pet owner by user ID: {UserId}", userId);
                
                var petOwner = await _petOwnerRepository.GetByUserIdAsync(userId).ConfigureAwait(false);
                if (petOwner == null)
                {
                    _logger.LogWarning("Pet owner not found for user: {UserId}", userId);
                    return Result<PetOwnerResponse>.Failure("Pet owner not found", "PET_OWNER_NOT_FOUND");
                }

                var response = await MapToPetOwnerResponseAsync(petOwner).ConfigureAwait(false);
                return Result<PetOwnerResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pet owner for user {UserId}", userId);
                return Result<PetOwnerResponse>.Failure("An error occurred while retrieving the pet owner", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<PetOwnerResponse>> CreatePetOwnerProfileAsync(CreatePetOwnerRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating pet owner profile for user {UserId}", request.UserId);

                // Validate user exists
                var user = await _userRepository.GetByIdAsync(request.UserId).ConfigureAwait(false);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", request.UserId);
                    return Result<PetOwnerResponse>.Failure("User not found", "USER_NOT_FOUND");
                }

                // Check if pet owner profile already exists
                var existingPetOwner = await _petOwnerRepository.GetByUserIdAsync(request.UserId).ConfigureAwait(false);
                if (existingPetOwner != null)
                {
                    _logger.LogWarning("Pet owner profile already exists for user: {UserId}", request.UserId);
                    return Result<PetOwnerResponse>.Failure(
                        "Pet owner profile already exists for this user", 
                        "PET_OWNER_ALREADY_EXISTS");
                }

                // Create pet owner
                var petOwner = new PetOwner
                {
                    UserId = request.UserId,
                    PreferredServiceTypes = request.PreferredServiceTypes,
                    PreferredProviderAttributes = request.PreferredProviderAttributes,
                    ReceiveMarketingEmails = request.ReceiveMarketingEmails,
                    ReceiveNotifications = true, // Default to true
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdPetOwner = await _petOwnerRepository.CreateAsync(petOwner).ConfigureAwait(false);

                _logger.LogInformation("Pet owner profile created successfully: {PetOwnerId}", createdPetOwner.Id);

                var response = await MapToPetOwnerResponseAsync(createdPetOwner).ConfigureAwait(false);
                return Result<PetOwnerResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating pet owner profile for user {UserId}", request.UserId);
                return Result<PetOwnerResponse>.Failure("An error occurred while creating the pet owner profile", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<PetOwnerResponse>> UpdatePetOwnerProfileAsync(UpdatePetOwnerRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating pet owner profile {PetOwnerId}", request.PetOwnerId);

                var existingPetOwner = await _petOwnerRepository.GetByIdAsync(request.PetOwnerId).ConfigureAwait(false);
                if (existingPetOwner == null)
                {
                    _logger.LogWarning("Pet owner not found: {PetOwnerId}", request.PetOwnerId);
                    return Result<PetOwnerResponse>.Failure("Pet owner not found", "PET_OWNER_NOT_FOUND");
                }

                // Update fields (only if provided)
                if (request.PreferredServiceTypes != null)
                {
                    existingPetOwner.PreferredServiceTypes = request.PreferredServiceTypes;
                }
                if (request.PreferredProviderAttributes != null)
                {
                    existingPetOwner.PreferredProviderAttributes = request.PreferredProviderAttributes;
                }
                if (request.ReceiveMarketingEmails.HasValue)
                {
                    existingPetOwner.ReceiveMarketingEmails = request.ReceiveMarketingEmails.Value;
                }
                if (request.ReceiveNotifications.HasValue)
                {
                    existingPetOwner.ReceiveNotifications = request.ReceiveNotifications.Value;
                }
                existingPetOwner.UpdatedAt = DateTime.UtcNow;

                await _petOwnerRepository.UpdateAsync(existingPetOwner).ConfigureAwait(false);

                _logger.LogInformation("Pet owner profile updated successfully: {PetOwnerId}", request.PetOwnerId);

                var response = await MapToPetOwnerResponseAsync(existingPetOwner).ConfigureAwait(false);
                return Result<PetOwnerResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating pet owner profile {PetOwnerId}", request.PetOwnerId);
                return Result<PetOwnerResponse>.Failure("An error occurred while updating the pet owner profile", "INTERNAL_ERROR");
            }
        }

        // Helper method to map PetOwner to PetOwnerResponse
        private async Task<PetOwnerResponse> MapToPetOwnerResponseAsync(PetOwner petOwner)
        {
            var pets = await _petRepository.GetByOwnerIdAsync(petOwner.Id).ConfigureAwait(false);
            var petCount = pets?.Count() ?? 0;

            var bookings = await _bookingRepository.GetByOwnerIdAsync(petOwner.Id).ConfigureAwait(false);
            var bookingCount = bookings?.Count() ?? 0;

            return new PetOwnerResponse
            {
                Id = petOwner.Id,
                UserId = petOwner.UserId,
                UserName = $"{petOwner.User?.FirstName} {petOwner.User?.LastName}".Trim(),
                UserEmail = petOwner.User?.Email ?? string.Empty,
                PreferredServiceTypes = petOwner.PreferredServiceTypes,
                PreferredProviderAttributes = petOwner.PreferredProviderAttributes,
                ReceiveMarketingEmails = petOwner.ReceiveMarketingEmails,
                ReceiveNotifications = petOwner.ReceiveNotifications,
                CreatedAt = petOwner.CreatedAt,
                UpdatedAt = petOwner.UpdatedAt,
                PetCount = petCount,
                BookingCount = bookingCount
            };
        }
    }
}