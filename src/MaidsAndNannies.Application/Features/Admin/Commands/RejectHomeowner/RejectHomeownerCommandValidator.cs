using FluentValidation;

namespace MaidsAndNannies.Application.Features.Admin.Commands.RejectHomeowner;

public sealed class RejectHomeownerCommandValidator : AbstractValidator<RejectHomeownerCommand>
{
    public RejectHomeownerCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().WithMessage("سبب الرفض مطلوب");
    }
}
