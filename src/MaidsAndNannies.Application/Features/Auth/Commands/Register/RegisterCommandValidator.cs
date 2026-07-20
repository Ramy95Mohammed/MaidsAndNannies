using FluentValidation;

namespace MaidsAndNannies.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.PhoneNumber).NotEmpty();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.PreferredLanguage).Must(l => l is "ar" or "en")
            .WithMessage("اللغة المفضلة يجب أن تكون 'ar' أو 'en'.");
    }
}
