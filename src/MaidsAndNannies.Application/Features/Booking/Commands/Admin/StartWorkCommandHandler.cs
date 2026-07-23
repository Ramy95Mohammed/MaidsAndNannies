using MaidsAndNannies.Application.Common.Interfaces;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.Admin;

public sealed class StartWorkCommandHandler(
    IApplicationDbContext dbContext)
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

        var worker = await dbContext.WorkerProfiles.FirstOrDefaultAsync(w => w.UserId == booking.WorkerId , ct);
        if(worker != null)
        {
            worker.IsAvailable = false;
        }

        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}