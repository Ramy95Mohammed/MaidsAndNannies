using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Policies.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Policies.Queries.GetPolicies;

public sealed record GetPoliciesQuery() : IRequest<List<PolicyDto>>;

public sealed class GetPoliciesQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetPoliciesQuery, List<PolicyDto>>
{
    public async Task<List<PolicyDto>> Handle(GetPoliciesQuery request, CancellationToken ct)
    {
        return await dbContext.Policies
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .Select(p => new PolicyDto(
                p.Key, p.TitleAr, p.TitleEn, p.ContentAr, p.ContentEn, p.SortOrder))
            .ToListAsync(ct);
    }
}