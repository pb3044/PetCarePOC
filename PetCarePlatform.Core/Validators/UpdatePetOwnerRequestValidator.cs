using FluentValidation;
using PetCarePlatform.Core.DTOs.Requests;

namespace PetCarePlatform.Core.Validators
{
    public class UpdatePetOwnerRequestValidator : AbstractValidator<UpdatePetOwnerRequest>
    {
        public UpdatePetOwnerRequestValidator()
        {
            RuleFor(x => x.PetOwnerId)
                .GreaterThan(0)
                .WithMessage("Pet owner ID is required");
        }
    }
}

