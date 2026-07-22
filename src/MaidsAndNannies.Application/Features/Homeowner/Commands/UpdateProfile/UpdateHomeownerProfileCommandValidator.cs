using FluentValidation;

namespace MaidsAndNannies.Application.Features.Homeowner.Commands.UpdateProfile;

public sealed class UpdateHomeownerProfileCommandValidator : AbstractValidator<UpdateHomeownerProfileCommand>
{
    public UpdateHomeownerProfileCommandValidator()
    {
        RuleFor(x => x.WhatsAppNumber)
          .NotEmpty()
          .WithMessage("WhatsApp number is required.");

        RuleFor(x => x.State)
         .NotEmpty()
         .WithMessage("State number is required.");

        When(x => x.NationalIdNumber is not null, () =>
            RuleFor(x => x.NationalIdNumber).MaximumLength(20));
        When(x => x.Address is not null, () =>
            RuleFor(x => x.Address).MaximumLength(500));
        When(x => x.City is not null, () =>
            RuleFor(x => x.City).MaximumLength(100));
        When(x => x.District is not null, () =>
            RuleFor(x => x.District).MaximumLength(100));
    }
}