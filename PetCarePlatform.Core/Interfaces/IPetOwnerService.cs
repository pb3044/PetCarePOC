using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PetCarePlatform.Core.Common;
using PetCarePlatform.Core.DTOs.Requests;
using PetCarePlatform.Core.DTOs.Responses;
using PetCarePlatform.Core.Models;

namespace PetCarePlatform.Core.Interfaces
{
    public interface IPetOwnerService
    {
        Task<Result<PetOwnerResponse>> GetPetOwnerByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Result<PetOwnerResponse>> GetPetOwnerByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<Result<PetOwnerResponse>> CreatePetOwnerProfileAsync(CreatePetOwnerRequest request, CancellationToken cancellationToken = default);
        Task<Result<PetOwnerResponse>> UpdatePetOwnerProfileAsync(UpdatePetOwnerRequest request, CancellationToken cancellationToken = default);
    }
}
