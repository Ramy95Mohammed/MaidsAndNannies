using FluentValidation;

namespace MaidsAndNannies.Application.Features.Auth.Commands.RegisterHomeowner;

public sealed class RegisterHomeownerCommandValidator : AbstractValidator<RegisterHomeownerCommand>
{
    public RegisterHomeownerCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.PhoneNumber).NotEmpty();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
