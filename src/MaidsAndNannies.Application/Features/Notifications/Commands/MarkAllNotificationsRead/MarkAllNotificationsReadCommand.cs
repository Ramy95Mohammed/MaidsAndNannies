using MaidsAndNannies.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Notifications.Commands.MarkAllNotificationsRead;

public sealed record MarkAllNotificationsReadCommand(string UserId) : IRequest<Unit>;

public sealed class MarkAllNotificationsReadCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<MarkAllNotificationsReadCommand, Unit>
{
    public async Task<Unit> Handle(MarkAllNotificationsReadCommand r, CancellationToken ct)
    {
        await dbContext.Notifications
            .Where(n => n.UserId == r.UserId && !n.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true), ct);
        return Unit.Value;
    }
}