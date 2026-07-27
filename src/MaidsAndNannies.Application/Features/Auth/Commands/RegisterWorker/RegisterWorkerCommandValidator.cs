using FluentValidation;

namespace MaidsAndNannies.Application.Features.Auth.Commands.RegisterWorker;

public sealed class RegisterWorkerCommandValidator : AbstractValidator<RegisterWorkerCommand>
{
    public RegisterWorkerCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.PhoneNumber).NotEmpty();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password)
            .WithMessage("كلمتا المرور غير متطابقتين");
        RuleFor(x => x.ExperienceYears).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MonthlyRate).GreaterThanOrEqualTo(0);
    }
}
