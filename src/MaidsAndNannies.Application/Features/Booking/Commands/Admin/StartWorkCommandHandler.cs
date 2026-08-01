using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Notifications;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.Admin;

public sealed class StartWorkCommandHandler(
    IApplicationDbContext dbContext ,
    INotificationService notifications)
    : IRequestHandler<StartWorkCommand, Unit>
{
    public async Task<Unit> Handle(StartWorkCommand request, CancellationToken ct)
    {
        var booking = await dbContext.Bookings.FindAsync([request.BookingId], ct)
            ?? throw new KeyNotFoundException("الحجز غير موجود");

        if (booking.Status != BookingStatus.Paid)
            throw new InvalidOperationException("يجب تأكيد الدفع أولاً");

        booking.Status = BookingStatus.Active;
        booking.UpdatedAt = DateTime.UtcNow;      

        await dbContext.SaveChangesAsync(ct);

        await notifications.NotifyAsync(booking.HomeownerId, NotificationType.WorkStarted, "NOTIF.WORK_STARTED",
    new { BookingId = booking.Id }, ct);

        return Unit.Value;
    }
}