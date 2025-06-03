using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Services
{
    public class PetOwnerService : IPetOwnerService
    {
        private readonly IPetOwnerRepository _petOwnerRepository;
        private readonly IPetRepository _petRepository;
        
        public PetOwnerService(IPetOwnerRepository petOwnerRepository, IPetRepository petRepository)
        {
            _petOwnerRepository = petOwnerRepository;
            _petRepository = petRepository;
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
            var existingPetOwner = await _petOwnerRepository.GetByIdAsync(petOwner.Id);
            if (existingPetOwner == null)
            {
                throw new InvalidOperationException("Pet owner profile not found");
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
            var existingPet = await _petRepository.GetByIdAsync(pet.Id);
            if (existingPet == null)
            {
                throw new InvalidOperationException("Pet not found");
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
            // This would typically filter by status based on includeHistory parameter
            // For now, we'll just return all bookings for the owner
            return (IEnumerable<Booking>)await _petOwnerRepository.GetByIdAsync(ownerId); //.Bookings;
        }
    }
}
