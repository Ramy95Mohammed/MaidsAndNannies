using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Admin.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Admin.Queries.GetAllHomeowners;

public sealed class GetAllHomeownersQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAllHomeownersQuery, IReadOnlyList<AdminHomeownerDto>>
{
    public async Task<IReadOnlyList<AdminHomeownerDto>> Handle(GetAllHomeownersQuery request, CancellationToken ct)
        => await dbContext.HomeownerProfiles
            .Include(h => h.User)
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new AdminHomeownerDto(
                h.Id,
                h.UserId,
                h.User.FullName,
                h.User.Email,
                h.User.PhoneNumber,
                h.VerificationStatus,
                h.MaxFaultReplacementCount,
                h.MaxPreferenceReplacementCount,
                h.CreatedAt))
            .ToListAsync(ct);
}