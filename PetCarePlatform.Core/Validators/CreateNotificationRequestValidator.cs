using FluentValidation;
using PetCarePlatform.Core.DTOs.Requests;

namespace PetCarePlatform.Core.Validators
{
    public class CreateNotificationRequestValidator : AbstractValidator<CreateNotificationRequest>
    {
        public CreateNotificationRequestValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .WithMessage("User ID is required");

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Notification title is required")
                .MaximumLength(200)
                .WithMessage("Title cannot exceed 200 characters");

            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Notification content is required")
                .MaximumLength(1000)
                .WithMessage("Content cannot exceed 1000 characters");

            RuleFor(x => x.ActionUrl)
                .MaximumLength(500)
                .WithMessage("Action URL cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.ActionUrl));
        }
    }
}

