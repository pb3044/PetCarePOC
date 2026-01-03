using FluentValidation;
using PetCarePlatform.Core.DTOs.Requests;

namespace PetCarePlatform.Core.Validators
{
    public class CancelBookingRequestValidator : AbstractValidator<CancelBookingRequest>
    {
        public CancelBookingRequestValidator()
        {
            RuleFor(x => x.BookingId)
                .GreaterThan(0)
                .WithMessage("Booking ID is required");

            RuleFor(x => x.CancellationReason)
                .NotEmpty()
                .WithMessage("Cancellation reason is required")
                .MinimumLength(10)
                .WithMessage("Cancellation reason must be at least 10 characters")
                .MaximumLength(500)
                .WithMessage("Cancellation reason cannot exceed 500 characters");
        }
    }
}

