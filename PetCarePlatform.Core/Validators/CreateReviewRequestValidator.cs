using FluentValidation;
using PetCarePlatform.Core.DTOs.Requests;

namespace PetCarePlatform.Core.Validators
{
    public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
    {
        public CreateReviewRequestValidator()
        {
            RuleFor(x => x.BookingId)
                .GreaterThan(0)
                .WithMessage("Booking ID is required");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5");

            RuleFor(x => x.Comment)
                .MaximumLength(2000)
                .WithMessage("Comment cannot exceed 2000 characters")
                .When(x => !string.IsNullOrWhiteSpace(x.Comment));

            RuleFor(x => x.PhotoUrls)
                .Must(photos => photos == null || photos.Count <= 5)
                .WithMessage("Maximum 5 photos allowed")
                .When(x => x.PhotoUrls != null);
        }
    }
}

