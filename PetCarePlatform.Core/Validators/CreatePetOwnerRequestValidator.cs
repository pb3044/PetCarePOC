using FluentValidation;
using PetCarePlatform.Core.DTOs.Requests;

namespace PetCarePlatform.Core.Validators
{
    public class CreatePetOwnerRequestValidator : AbstractValidator<CreatePetOwnerRequest>
    {
        public CreatePetOwnerRequestValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .WithMessage("User ID is required");
        }
    }
}

