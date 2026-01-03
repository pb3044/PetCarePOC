using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PetCarePlatform.Core.Common;
using PetCarePlatform.Core.DTOs.Requests;
using PetCarePlatform.Core.DTOs.Responses;
using PetCarePlatform.Core.Exceptions;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using System.Security.Cryptography;
using System.Text;

namespace PetCarePlatform.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;
        
        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApplicationUser> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<ApplicationUser> RegisterUserAsync(ApplicationUser user, string password)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new ArgumentException("Email cannot be null or empty", nameof(user));
            }

            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(user.Email))
            {
                _logger.LogWarning("Registration attempt with existing email: {Email}", user.Email);
                throw new InvalidOperationException("Email is already registered");
            }

            // Hash the password
            user.PasswordHash = HashPassword(password);
            
            // Set default values
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.EmailConfirmed = false;
            //user.PhoneConfirmed = false;
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

        public async Task UpdateUserProfileAsync(ApplicationUser user)
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

        public Task GetUserByIdAsync(string? userId)
        {
            throw new NotImplementedException();
        }

        // ============================================
        // Enterprise Pattern Methods (Result-based)
        // ============================================

        public async Task<Result<UserResponse>> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting user by ID: {UserId}", id);
                
                var user = await _userRepository.GetByIdAsync(id).ConfigureAwait(false);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", id);
                    return Result<UserResponse>.Failure("User not found", "USER_NOT_FOUND");
                }

                var response = MapToUserResponse(user);
                return Result<UserResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", id);
                return Result<UserResponse>.Failure("An error occurred while retrieving the user", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<UserResponse>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting user by email: {Email}", email);
                
                var user = await _userRepository.GetByEmailAsync(email).ConfigureAwait(false);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {Email}", email);
                    return Result<UserResponse>.Failure("User not found", "USER_NOT_FOUND");
                }

                var response = MapToUserResponse(user);
                return Result<UserResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email {Email}", email);
                return Result<UserResponse>.Failure("An error occurred while retrieving the user", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<UserResponse>> RegisterUserAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Registering user with email: {Email}", request.Email);

                // Check if email already exists
                if (await _userRepository.EmailExistsAsync(request.Email).ConfigureAwait(false))
                {
                    _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
                    return Result<UserResponse>.Failure("Email is already registered", "EMAIL_ALREADY_EXISTS");
                }

                // Create user
                var user = new ApplicationUser
                {
                    Email = request.Email,
                    PasswordHash = HashPassword(request.Password),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    EmailConfirmed = false,
                    IsActive = true
                };

                var createdUser = await _userRepository.CreateAsync(user).ConfigureAwait(false);

                _logger.LogInformation("User registered successfully: {UserId}", createdUser.Id);

                var response = MapToUserResponse(createdUser);
                return Result<UserResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user with email {Email}", request.Email);
                return Result<UserResponse>.Failure("An error occurred while registering the user", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<bool>> ValidateUserCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Validating user credentials for email: {Email}", email);

                var user = await _userRepository.GetByEmailAsync(email).ConfigureAwait(false);
                if (user == null)
                {
                    _logger.LogWarning("User not found for credential validation: {Email}", email);
                    return Result<bool>.Success(false);
                }

                var isValid = VerifyPassword(password, user.PasswordHash);
                return Result<bool>.Success(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user credentials for email {Email}", email);
                return Result<bool>.Failure("An error occurred while validating credentials", "INTERNAL_ERROR");
            }
        }

        public async Task<Result<UserResponse>> UpdateUserProfileAsync(UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating user profile: {UserId}", request.UserId);

                var existingUser = await _userRepository.GetByIdAsync(request.UserId).ConfigureAwait(false);
                if (existingUser == null)
                {
                    _logger.LogWarning("User not found: {UserId}", request.UserId);
                    return Result<UserResponse>.Failure("User not found", "USER_NOT_FOUND");
                }

                // Update only allowed fields
                existingUser.FirstName = request.FirstName;
                existingUser.LastName = request.LastName;
                existingUser.PhoneNumber = request.PhoneNumber;
                existingUser.ProfilePhotoUrl = request.ProfilePhotoUrl;
                existingUser.Address = request.Address;
                existingUser.City = request.City;
                existingUser.Province = request.Province;
                existingUser.PostalCode = request.PostalCode;
                existingUser.Latitude = request.Latitude;
                existingUser.Longitude = request.Longitude;
                existingUser.Bio = request.Bio;
                existingUser.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(existingUser).ConfigureAwait(false);

                _logger.LogInformation("User profile updated successfully: {UserId}", request.UserId);

                var response = MapToUserResponse(existingUser);
                return Result<UserResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile {UserId}", request.UserId);
                return Result<UserResponse>.Failure("An error occurred while updating the user profile", "INTERNAL_ERROR");
            }
        }

        public async Task<Result> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Changing password for user: {UserId}", request.UserId);

                var user = await _userRepository.GetByIdAsync(request.UserId).ConfigureAwait(false);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", request.UserId);
                    return Result.Failure("User not found", "USER_NOT_FOUND");
                }

                if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
                {
                    _logger.LogWarning("Incorrect current password for user: {UserId}", request.UserId);
                    return Result.Failure("Current password is incorrect", "INVALID_PASSWORD");
                }

                user.PasswordHash = HashPassword(request.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user).ConfigureAwait(false);

                _logger.LogInformation("Password changed successfully for user: {UserId}", request.UserId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", request.UserId);
                return Result.Failure("An error occurred while changing the password", "INTERNAL_ERROR");
            }
        }

        // Helper method to map ApplicationUser to UserResponse
        private UserResponse MapToUserResponse(ApplicationUser user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                ProfilePhotoUrl = user.ProfilePhotoUrl,
                Address = user.Address,
                City = user.City,
                Province = user.Province,
                PostalCode = user.PostalCode,
                Latitude = user.Latitude,
                Longitude = user.Longitude,
                Bio = user.Bio,
                UserType = user.UserType,
                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        public async Task<Result> DeactivateUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var userResult = await GetUserByIdAsync(userId, cancellationToken);
                if (userResult.IsFailure || userResult.Value == null)
                {
                    return Result.Failure("User not found");
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return Result.Failure("User not found");
                }

                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", userId);
                return Result.Failure($"Error deactivating user: {ex.Message}");
            }
        }

        public async Task<Result> ReactivateUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var userResult = await GetUserByIdAsync(userId, cancellationToken);
                if (userResult.IsFailure || userResult.Value == null)
                {
                    return Result.Failure("User not found");
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return Result.Failure("User not found");
                }

                user.IsActive = true;
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating user {UserId}", userId);
                return Result.Failure($"Error reactivating user: {ex.Message}");
            }
        }
    }
}
