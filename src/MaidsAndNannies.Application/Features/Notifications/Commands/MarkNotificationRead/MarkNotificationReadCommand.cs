using MaidsAndNannies.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Notifications.Commands.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(string UserId, int NotificationId) : IRequest<Unit>;

public sealed class MarkNotificationReadCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<MarkNotificationReadCommand, Unit>
{
    public async Task<Unit> Handle(MarkNotificationReadCommand r, CancellationToken ct)
    {
        var notification = await dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == r.NotificationId && n.UserId == r.UserId, ct)
            ?? throw new KeyNotFoundException("الإشعار غير موجود");

        notification.IsRead = true;
        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}