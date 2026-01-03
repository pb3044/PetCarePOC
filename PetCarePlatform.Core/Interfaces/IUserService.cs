using System.Threading;
using System.Threading.Tasks;
using PetCarePlatform.Core.Common;
using PetCarePlatform.Core.DTOs.Requests;
using PetCarePlatform.Core.DTOs.Responses;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IUserService
    {
        Task<Result<UserResponse>> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<UserResponse>> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Result<UserResponse>> RegisterUserAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
        Task<Result<bool>> ValidateUserCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);
        Task<Result<UserResponse>> UpdateUserProfileAsync(UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
        Task<Result> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);
        Task<Result> DeactivateUserAsync(int userId, CancellationToken cancellationToken = default);
        Task<Result> ReactivateUserAsync(int userId, CancellationToken cancellationToken = default);
    }
}
