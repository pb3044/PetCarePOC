using FluentValidation;
using PetCarePlatform.Core.DTOs.Requests;

namespace PetCarePlatform.Core.Validators
{
    public class CreateServiceProviderRequestValidator : AbstractValidator<CreateServiceProviderRequest>
    {
        public CreateServiceProviderRequestValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .WithMessage("User ID is required");

            RuleFor(x => x.BusinessName)
                .NotEmpty()
                .WithMessage("Business name is required")
                .MaximumLength(200)
                .WithMessage("Business name cannot exceed 200 characters");

            RuleFor(x => x.BusinessType)
                .NotEmpty()
                .WithMessage("Business type is required")
                .MaximumLength(100)
                .WithMessage("Business type cannot exceed 100 characters");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Description is required")
                .MinimumLength(20)
                .WithMessage("Description must be at least 20 characters")
                .MaximumLength(2000)
                .WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.ServiceRadius)
                .GreaterThan(0)
                .WithMessage("Service radius must be greater than 0")
                .LessThanOrEqualTo(100)
                .WithMessage("Service radius cannot exceed 100 km");
        }
    }
}

