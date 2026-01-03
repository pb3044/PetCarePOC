using FluentValidation;
using PetCarePlatform.Core.DTOs.Requests;

namespace PetCarePlatform.Core.Validators
{
    public class CreatePaymentIntentRequestValidator : AbstractValidator<CreatePaymentIntentRequest>
    {
        public CreatePaymentIntentRequestValidator()
        {
            RuleFor(x => x.BookingId)
                .GreaterThan(0)
                .WithMessage("Booking ID is required");
        }
    }
}

