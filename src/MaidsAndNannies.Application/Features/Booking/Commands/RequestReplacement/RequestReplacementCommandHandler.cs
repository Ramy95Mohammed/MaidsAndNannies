using MaidsAndNannies.Application.Common.Interfaces;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.RequestReplacement;

public sealed class RequestReplacementCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<RequestReplacementCommand, Unit>
{
    public async Task<Unit> Handle(RequestReplacementCommand request, CancellationToken ct)
    {
        var booking = await dbContext.Bookings
            .FirstOrDefaultAsync(b => b.Id == request.BookingId && b.HomeownerId == request.HomeownerId, ct)
            ?? throw new KeyNotFoundException("الحجز غير موجود");

        if (booking.ReplacementCount >= 2)
            throw new InvalidOperationException("تم تجاوز الحد الأقصى للاستبدال (مرتين)");

        if (booking.Status != BookingStatus.Paid && booking.Status != BookingStatus.Active)
            throw new InvalidOperationException("لا يمكن طلب استبدال في هذه الحالة");

        var newWorker = await dbContext.WorkerProfiles
            .FirstOrDefaultAsync(w => w.Id == request.NewWorkerId, ct)
            ?? throw new KeyNotFoundException("العاملة غير موجودة");

        booking.WorkerId = newWorker.UserId;
        booking.ReplacementCount++;
        booking.Status = BookingStatus.ReplacementRequested;
        booking.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}