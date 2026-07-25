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

        var maxReplacementStr = await dbContext.AppSettings
    .Where(s => s.Key == "MaxReplacementCount")
    .Select(s => s.Value)
    .FirstOrDefaultAsync(ct);
        var maxReplacement = int.TryParse(maxReplacementStr, out var max) ? max : 2;

        if (booking.ReplacementCount >= maxReplacement)
            throw new InvalidOperationException($"تم تجاوز الحد الأقصى للاستبدال ({maxReplacement} مرات)");


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