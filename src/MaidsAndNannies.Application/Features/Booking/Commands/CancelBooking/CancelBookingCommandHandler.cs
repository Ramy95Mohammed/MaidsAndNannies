using MaidsAndNannies.Application.Common.Interfaces;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.CancelBooking;

public sealed class CancelBookingCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<CancelBookingCommand, Unit>
{
    public async Task<Unit> Handle(CancelBookingCommand request, CancellationToken ct)
    {
        var booking = await dbContext.Bookings
            .FirstOrDefaultAsync(b => b.Id == request.BookingId && b.HomeownerId == request.UserId, ct)
            ?? throw new KeyNotFoundException("الحجز غير موجود");

        if (booking.Status != BookingStatus.Pending)
            throw new InvalidOperationException("لا يمكن إلغاء الحجز في هذه الحالة");

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;

        var worker = await dbContext.WorkerProfiles
              .FirstOrDefaultAsync(w => w.UserId == booking.WorkerId, ct);
        if (worker is not null)
            worker.IsAvailable = true;

        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}