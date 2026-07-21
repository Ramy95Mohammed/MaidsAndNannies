using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Admin.Common;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Admin.Queries.GetPendingHomeowners;

public sealed class GetPendingHomeownersQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetPendingHomeownersQuery, IReadOnlyList<PendingHomeownerDto>>
{
    public async Task<IReadOnlyList<PendingHomeownerDto>> Handle(GetPendingHomeownersQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.HomeownerProfiles
            .Include(h => h.User)
            .Where(h => h.VerificationStatus == VerificationStatus.Pending)
            .OrderBy(h => h.CreatedAt)
            .Select(h => new PendingHomeownerDto(
                h.Id,
                h.UserId,
                h.User.FullName,
                h.User.Email,
                h.User.PhoneNumber,
                h.City,
                h.Address,
                h.VerificationStatus,
                h.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
