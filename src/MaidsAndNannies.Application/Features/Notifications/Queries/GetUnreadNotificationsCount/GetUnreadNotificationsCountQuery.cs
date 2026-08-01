using MaidsAndNannies.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Notifications.Queries.GetUnreadNotificationsCount;

public sealed record GetUnreadNotificationsCountQuery(string UserId) : IRequest<int>;

public sealed class GetUnreadNotificationsCountQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetUnreadNotificationsCountQuery, int>
{
    public async Task<int> Handle(GetUnreadNotificationsCountQuery r, CancellationToken ct)
    {
        return await dbContext.Notifications
            .CountAsync(n => n.UserId == r.UserId && !n.IsRead, ct);
    }
}