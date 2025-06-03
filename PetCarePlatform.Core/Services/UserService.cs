using System;
using System.Threading.Tasks;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using System.Security.Cryptography;
using System.Text;

namespace PetCarePlatform.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<User> RegisterUserAsync(User user, string password)
        {
            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(user.Email))
            {
                throw new InvalidOperationException("Email is already registered");
            }

            // Hash the password
            user.PasswordHash = HashPassword(password);
            
            // Set default values
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.EmailConfirmed = false;
            user.PhoneConfirmed = false;
            user.IsActive = true;

            // Create the user
            return await _userRepository.CreateAsync(user);
        }

        public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            return VerifyPassword(password, user.PasswordHash);
        }

        public async Task UpdateUserProfileAsync(User user)
        {
            var existingUser = await _userRepository.GetByIdAsync(user.Id);
            if (existingUser == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Update only allowed fields
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.ProfilePhotoUrl = user.ProfilePhotoUrl;
            existingUser.Address = user.Address;
            existingUser.City = user.City;
            existingUser.Province = user.Province;
            existingUser.PostalCode = user.PostalCode;
            existingUser.Latitude = user.Latitude;
            existingUser.Longitude = user.Longitude;
            existingUser.Bio = user.Bio;
            existingUser.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(existingUser);
        }

        public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            if (!VerifyPassword(currentPassword, user.PasswordHash))
            {
                throw new InvalidOperationException("Current password is incorrect");
            }

            user.PasswordHash = HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
        }

        public async Task RequestPasswordResetAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                return;
            }

            // In a real implementation, generate a token and send an email
            // For now, we'll just update the user record
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }

        public async Task ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                throw new InvalidOperationException("Invalid reset request");
            }

            // In a real implementation, validate the token
            // For now, we'll just update the password
            user.PasswordHash = HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
        }

        public async Task ConfirmEmailAsync(string email, string token)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // In a real implementation, validate the token
            // For now, we'll just mark the email as confirmed
            user.EmailConfirmed = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
        }

        public async Task<bool> IsEmailConfirmedAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            return user.EmailConfirmed;
        }

        public async Task DeactivateUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
        }

        public async Task ReactivateUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}
