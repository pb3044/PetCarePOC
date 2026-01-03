using FluentValidation;
using PetCarePlatform.Core.DTOs.Requests;

namespace PetCarePlatform.Core.Validators
{
    public class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
    {
        public SendMessageRequestValidator()
        {
            RuleFor(x => x.ReceiverId)
                .GreaterThan(0)
                .WithMessage("Receiver ID is required");

            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Message content is required")
                .MaximumLength(5000)
                .WithMessage("Message content cannot exceed 5000 characters")
                .MinimumLength(1)
                .WithMessage("Message content cannot be empty");

            RuleFor(x => x.BookingId)
                .GreaterThan(0)
                .WithMessage("Booking ID must be valid")
                .When(x => x.BookingId.HasValue);

            RuleFor(x => x.AttachmentUrls)
                .Must(attachments => attachments == null || attachments.Count <= 5)
                .WithMessage("Maximum 5 attachments allowed")
                .When(x => x.AttachmentUrls != null);
        }
    }
}

