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

        var maxSetting = await dbContext.AppSettings
            .FirstOrDefaultAsync(s => s.Key == "MaxReplacementCount", ct);
        var max = int.TryParse(maxSetting?.Value, out var m) ? m : 2;
        if (booking.ReplacementCount >= max)
            throw new InvalidOperationException($"تم تجاوز الحد الأقصى للاستبدال ({max} مرات)");

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
        booking.ReplacementCount++;
        booking.Status = BookingStatus.Pending;
        booking.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }   
}