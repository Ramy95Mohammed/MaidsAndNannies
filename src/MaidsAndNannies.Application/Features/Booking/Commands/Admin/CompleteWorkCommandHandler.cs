using MaidsAndNannies.Application.Common.Interfaces;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.Admin;

public sealed class CompleteWorkCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<CompleteWorkCommand, Unit>
{
    public async Task<Unit> Handle(CompleteWorkCommand request, CancellationToken ct)
    {
        var booking = await dbContext.Bookings.FindAsync([request.BookingId], ct)
            ?? throw new KeyNotFoundException("الحجز غير موجود");

        if (booking.Status != BookingStatus.Active)
            throw new InvalidOperationException("الحجز ليس نشطاً");

        booking.Status = BookingStatus.Completed;
        booking.EndDate = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;

        var worker = await dbContext.WorkerProfiles
                .FirstOrDefaultAsync(w => w.UserId == booking.WorkerId, ct);
        if (worker is not null)
            worker.IsAvailable = true;

        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}