using FluentValidation;

namespace MaidsAndNannies.Application.Features.Worker.Commands.UpdateProfile;

public sealed class UpdateWorkerProfileCommandValidator : AbstractValidator<UpdateWorkerProfileCommand>
{
    public UpdateWorkerProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ExperienceYears).GreaterThanOrEqualTo(0).When(x => x.ExperienceYears.HasValue);
        RuleFor(x => x.HourlyRate).GreaterThanOrEqualTo(0).When(x => x.HourlyRate.HasValue);
        RuleFor(x => x.MonthlyRate).GreaterThanOrEqualTo(0).When(x => x.MonthlyRate.HasValue);
    }
}
