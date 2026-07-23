using MaidsAndNannies.Application.Common.Interfaces;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.Admin;

public sealed class ConfirmWorkerCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<ConfirmWorkerCommand, Unit>
{
    public async Task<Unit> Handle(ConfirmWorkerCommand request, CancellationToken ct)
    {
        var booking = await dbContext.Bookings.FindAsync([request.BookingId], ct)
            ?? throw new KeyNotFoundException("الحجز غير موجود");

        if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.ReplacementRequested)
            throw new InvalidOperationException("لا يمكن تأكيد العاملة في هذه الحالة");

        booking.Status = booking.Status == BookingStatus.ReplacementRequested
            ? BookingStatus.Paid       // تم الدفع مسبقاً
            : BookingStatus.WorkerConfirmed;
        booking.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}