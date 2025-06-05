using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IUserService
    {
        Task<ApplicationUser> GetUserByIdAsync(int id);
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<ApplicationUser> RegisterUserAsync(ApplicationUser user, string password);
        Task<bool> ValidateUserCredentialsAsync(string email, string password);
        Task UpdateUserProfileAsync(ApplicationUser user);
        Task ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task RequestPasswordResetAsync(string email);
        Task ResetPasswordAsync(string email, string token, string newPassword);
        Task ConfirmEmailAsync(string email, string token);
        Task<bool> IsEmailConfirmedAsync(string email);
        Task DeactivateUserAsync(int id);
        Task ReactivateUserAsync(int id);
    }
}
