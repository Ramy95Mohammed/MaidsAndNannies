using MaidsAndNannies.Application.Common.Interfaces;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.RequestReplacement;

public sealed class RequestReplacementCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<RequestReplacementCommand, Unit>
{

    public async Task<Unit> Handle(RequestReplacementCommand r, CancellationToken ct)
    {
        var booking = await dbContext.Bookings
            .FirstOrDefaultAsync(b => b.Id == r.BookingId && b.HomeownerId == r.HomeownerId, ct)
            ?? throw new KeyNotFoundException("الحجز غير موجود");

        var oldWorker = await dbContext.WorkerProfiles
    .FirstOrDefaultAsync(w => w.UserId == booking.WorkerId, ct);
        if (oldWorker is not null)
            oldWorker.IsAvailable = true;

        var maxSetting = await dbContext.AppSettings
            .FirstOrDefaultAsync(s => s.Key == "MaxReplacementCount", ct);
        var max = int.TryParse(maxSetting?.Value, out var m) ? m : 2;

        // تحديث آمن مع منع السباق
        var rows = await dbContext.Bookings
            .Where(b => b.Id == r.BookingId && b.ReplacementCount < max)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(b => b.ReplacementCount, b => b.ReplacementCount + 1), ct);
        if (rows == 0)
            throw new InvalidOperationException($"تم تجاوز الحد الأقصى للاستبدال ({max} مرات)");

        // إعادة قراءة الحجز بعد التحديث
        booking = await dbContext.Bookings.FirstAsync(b => b.Id == r.BookingId, ct);

        string newWorkerId;
        if (r.ApplicationId.HasValue)
        {
            var app = await dbContext.JobApplications
                .FirstOrDefaultAsync(a => a.Id == r.ApplicationId && a.JobPostId == booking.JobPostId, ct)
                ?? throw new KeyNotFoundException("الطلب غير موجود");
            newWorkerId = app.WorkerId;
        }
        else
        {
            var worker = await dbContext.WorkerProfiles
                .FirstOrDefaultAsync(w => w.Id == r.NewWorkerId, ct)
                ?? throw new KeyNotFoundException("العاملة غير موجودة");
            newWorkerId = worker.UserId;
        }

        booking.WorkerId = newWorkerId;
        booking.Status = booking.IsPaid ? BookingStatus.ReplacementRequested : BookingStatus.Pending;
        booking.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}