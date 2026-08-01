using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Notifications.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Notifications.Queries.GetMyNotifications;

public sealed record GetMyNotificationsQuery(string UserId) : IRequest<List<NotificationDto>>;

public sealed class GetMyNotificationsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetMyNotificationsQuery, List<NotificationDto>>
{
    public async Task<List<NotificationDto>> Handle(GetMyNotificationsQuery r, CancellationToken ct)
    {
        return await dbContext.Notifications
            .Where(n => n.UserId == r.UserId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new NotificationDto(n.Id, n.Type ?? string.Empty, n.Title, n.Message, n.IsRead, n.CreatedAt))
            .ToListAsync(ct);
    }
}