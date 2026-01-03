using FluentValidation;
using PetCarePlatform.Core.DTOs.Requests;

namespace PetCarePlatform.Core.Validators
{
    public class ConfirmPaymentRequestValidator : AbstractValidator<ConfirmPaymentRequest>
    {
        public ConfirmPaymentRequestValidator()
        {
            RuleFor(x => x.PaymentId)
                .GreaterThan(0)
                .WithMessage("Payment ID is required");

            RuleFor(x => x.TransactionId)
                .NotEmpty()
                .WithMessage("Transaction ID is required")
                .MaximumLength(200)
                .WithMessage("Transaction ID cannot exceed 200 characters");
        }
    }
}

