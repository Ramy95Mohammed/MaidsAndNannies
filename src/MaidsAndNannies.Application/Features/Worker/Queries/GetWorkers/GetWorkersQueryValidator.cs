using FluentValidation;

namespace MaidsAndNannies.Application.Features.Worker.Queries.GetWorkers;

public sealed class GetWorkersQueryValidator : AbstractValidator<GetWorkersQuery>
{
    public GetWorkersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
