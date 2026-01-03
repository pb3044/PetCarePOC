using FluentValidation;
using PetCarePlatform.Core.DTOs.Requests;

namespace PetCarePlatform.Core.Validators
{
    public class ProcessRefundRequestValidator : AbstractValidator<ProcessRefundRequest>
    {
        public ProcessRefundRequestValidator()
        {
            RuleFor(x => x.PaymentId)
                .GreaterThan(0)
                .WithMessage("Payment ID is required");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Refund amount must be greater than 0");

            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("Refund reason is required")
                .MinimumLength(10)
                .WithMessage("Refund reason must be at least 10 characters")
                .MaximumLength(500)
                .WithMessage("Refund reason cannot exceed 500 characters");
        }
    }
}

