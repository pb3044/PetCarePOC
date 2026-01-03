using FluentValidation;
using PetCarePlatform.Core.DTOs.Requests;

namespace PetCarePlatform.Core.Validators
{
    public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
    {
        public CreateBookingRequestValidator()
        {
            RuleFor(x => x.ServiceId)
                .GreaterThan(0)
                .WithMessage("Service ID is required");

            RuleFor(x => x.OwnerId)
                .GreaterThan(0)
                .WithMessage("Owner ID is required");

            RuleFor(x => x.StartTime)
                .NotEmpty()
                .WithMessage("Start time is required")
                .Must(BeInFuture)
                .WithMessage("Start time must be in the future")
                .When(x => x.StartTime != default);

            RuleFor(x => x.EndTime)
                .NotEmpty()
                .WithMessage("End time is required")
                .GreaterThan(x => x.StartTime)
                .WithMessage("End time must be after start time")
                .When(x => x.EndTime != default && x.StartTime != default);

            RuleFor(x => x.SpecialInstructions)
                .MaximumLength(1000)
                .WithMessage("Special instructions cannot exceed 1000 characters")
                .When(x => !string.IsNullOrEmpty(x.SpecialInstructions));

            RuleFor(x => x.TotalPrice)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Total price cannot be negative")
                .When(x => x.TotalPrice.HasValue);
        }

        private bool BeInFuture(DateTime dateTime)
        {
            return dateTime > DateTime.UtcNow;
        }
    }
}
