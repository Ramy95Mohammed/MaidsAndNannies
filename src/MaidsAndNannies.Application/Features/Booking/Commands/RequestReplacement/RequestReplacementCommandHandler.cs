using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Enums;
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
            .Include(b => b.Currency)
            .FirstOrDefaultAsync(b => b.Id == r.BookingId && b.HomeownerId == r.HomeownerId, ct)
            ?? throw new KeyNotFoundException("الحجز غير موجود");

        var maxSetting = await dbContext.AppSettings
            .FirstOrDefaultAsync(s => s.Key == "MaxReplacementCount", ct);
        var max = int.TryParse(maxSetting?.Value, out var m) ? m : 2;

        var rows = await dbContext.Bookings
            .Where(b => b.Id == r.BookingId && b.ReplacementCount < max)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(b => b.ReplacementCount, b => b.ReplacementCount + 1), ct);
        if (rows == 0)
            throw new InvalidOperationException($"تم تجاوز الحد الأقصى للاستبدال ({max} مرات)");

        booking = await dbContext.Bookings
            .Include(b => b.Currency)
            .FirstAsync(b => b.Id == r.BookingId, ct);

        // تحرير العاملة القديمة
        var oldWorker = await dbContext.WorkerProfiles
            .FirstOrDefaultAsync(w => w.UserId == booking.WorkerId, ct);
        if (oldWorker is not null)
            oldWorker.IsAvailable = true;

        string newWorkerId;
        int newWorkerProfileId;
        decimal newDailyRate, newHourlyRate, newMonthlyRate;
        int newCurrencyId;
        decimal newRateToEgp;

        if (r.ApplicationId.HasValue)
        {
            var app = await dbContext.JobApplications
                .Include(a => a.JobPost).ThenInclude(j => j.Currency)
                .FirstOrDefaultAsync(a => a.Id == r.ApplicationId && a.JobPostId == booking.JobPostId, ct)
                ?? throw new KeyNotFoundException("الطلب غير موجود");
            newWorkerId = app.WorkerId;
            newWorkerProfileId = 0;
            newDailyRate = app.JobPost.DailySalary;
            newHourlyRate = app.JobPost.HourlySalary;
            newMonthlyRate = app.JobPost.MonthlySalary;
            newCurrencyId = app.JobPost.CurrencyId;
            newRateToEgp = app.JobPost.Currency.RateToEgp;
        }
        else
        {
            var newWorker = await dbContext.WorkerProfiles
                .Include(w => w.Currency)
                .FirstOrDefaultAsync(w => w.Id == r.NewWorkerId, ct)
                ?? throw new KeyNotFoundException("العاملة غير موجودة");
            newWorkerId = newWorker.UserId;
            newWorkerProfileId = newWorker.Id;
            newDailyRate = newWorker.DailyRate ?? 0;
            newHourlyRate = newWorker.HourlyRate ?? 0;
            newMonthlyRate = newWorker.MonthlyRate ?? 0;
            newCurrencyId = newWorker.CurrencyId;
            newRateToEgp = newWorker.Currency.RateToEgp;
        }

        var oldRateToEgp = booking.Currency?.RateToEgp ?? 1m;

        var newTotal = booking.BookingType switch
        {
            BookingType.Daily => newDailyRate * booking.Quantity,
            BookingType.Hourly => newHourlyRate * booking.Quantity,
            BookingType.Monthly => newMonthlyRate,
            _ => newMonthlyRate
        };

        var oldTotalInEgp = booking.TotalAmount * oldRateToEgp;
        var newTotalInEgp = newTotal * newRateToEgp;
        var newCommissionInEgp = newTotalInEgp * (booking.CommissionAmount / (oldTotalInEgp > 0 ? oldTotalInEgp : 1));
        var oldCommissionInEgp = booking.CommissionAmount;

        // تحديث بيانات الحجز
        booking.WorkerId = newWorkerId;
        booking.OriginalWorkerId = newWorkerProfileId;
        booking.MonthlySalary = newMonthlyRate;
        booking.DailySalary = newDailyRate;
        booking.HourlySalary = newHourlyRate;
        booking.TotalAmount = newTotal;
        booking.CurrencyId = newCurrencyId;

        if (booking.IsPaid)
        {
            if (newCommissionInEgp > oldCommissionInEgp)
            {
                booking.OutstandingAmount = newCommissionInEgp - oldCommissionInEgp;
                booking.Status = BookingStatus.ReplacementRequested;
            }
            else
            {
                booking.OutstandingAmount = 0;
                booking.Status = BookingStatus.ReplacementRequested;
            }
        }
        else
        {
            booking.CommissionAmount = newCommissionInEgp;
            booking.Status = BookingStatus.Pending;
        }

        booking.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}