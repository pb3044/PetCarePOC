using FluentValidation;
using PetCarePlatform.Core.DTOs.Requests;

namespace PetCarePlatform.Core.Validators
{
    public class UpdateReviewRequestValidator : AbstractValidator<UpdateReviewRequest>
    {
        public UpdateReviewRequestValidator()
        {
            RuleFor(x => x.ReviewId)
                .GreaterThan(0)
                .WithMessage("Review ID is required");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5");

            RuleFor(x => x.Comment)
                .MaximumLength(2000)
                .WithMessage("Comment cannot exceed 2000 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Comment));
        }
    }
}

