using FluentValidation;
using PetCarePlatform.Core.DTOs.Requests;

namespace PetCarePlatform.Core.Validators
{
    public class CreateServiceRequestValidator : AbstractValidator<CreateServiceRequest>
    {
        public CreateServiceRequestValidator()
        {
            RuleFor(x => x.ProviderId)
                .GreaterThan(0)
                .WithMessage("Provider ID is required");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Service title is required")
                .MaximumLength(200)
                .WithMessage("Title cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Service description is required")
                .MinimumLength(20)
                .WithMessage("Description must be at least 20 characters")
                .MaximumLength(2000)
                .WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.BasePrice)
                .GreaterThan(0)
                .WithMessage("Base price must be greater than 0");

            RuleFor(x => x.PriceUnit)
                .NotEmpty()
                .WithMessage("Price unit is required")
                .Must(BeValidPriceUnit)
                .WithMessage("Price unit must be 'per hour', 'per day', or 'per visit'");

            RuleFor(x => x.MaxPetsPerBooking)
                .GreaterThan(0)
                .WithMessage("Max pets per booking must be greater than 0")
                .When(x => x.MaxPetsPerBooking.HasValue);
        }

        private bool BeValidPriceUnit(string priceUnit)
        {
            return priceUnit == "per hour" || priceUnit == "per day" || priceUnit == "per visit";
        }
    }
}
