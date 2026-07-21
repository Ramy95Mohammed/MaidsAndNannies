using FluentValidation;

namespace MaidsAndNannies.Application.Features.Admin.Commands.RejectWorker;

public sealed class RejectWorkerCommandValidator : AbstractValidator<RejectWorkerCommand>
{
    public RejectWorkerCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().WithMessage("سبب الرفض مطلوب");
    }
}
