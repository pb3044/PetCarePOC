using FluentValidation;
using PetCarePlatform.Core.DTOs.Requests;

namespace PetCarePlatform.Core.Validators
{
    public class AddReviewResponseRequestValidator : AbstractValidator<AddReviewResponseRequest>
    {
        public AddReviewResponseRequestValidator()
        {
            RuleFor(x => x.ReviewId)
                .GreaterThan(0)
                .WithMessage("Review ID is required");

            RuleFor(x => x.Response)
                .NotEmpty()
                .WithMessage("Response is required")
                .MaximumLength(1000)
                .WithMessage("Response cannot exceed 1000 characters");
        }
    }
}

