using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.RequestReplacement;

public sealed class RequestReplacementCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<RequestReplacementCommand, Unit>
{
    private const int BillingPeriodDays = 30;

    public async Task<Unit> Handle(RequestReplacementCommand r, CancellationToken ct)
    {
        var booking = await dbContext.Bookings
            .Include(b => b.Currency)
            .FirstOrDefaultAsync(b => b.Id == r.BookingId && b.HomeownerId == r.HomeownerId, ct)
            ?? throw new KeyNotFoundException("الحجز غير موجود");

        // ── حد أقصى للاستبدال ──
        var maxKey = r.Reason == ReplacementReason.WorkerFault
            ? "MaxFaultReplacementCount"
            : "MaxPreferenceReplacementCount";
        var maxDefault = r.Reason == ReplacementReason.WorkerFault ? 3 : 1;

        var maxSetting = await dbContext.AppSettings.FirstOrDefaultAsync(s => s.Key == maxKey, ct);
        var max = int.TryParse(maxSetting?.Value, out var m) ? m : maxDefault;

        var rows = await dbContext.Bookings
            .Where(b => b.Id == r.BookingId && b.ReplacementCount < max)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(b => b.ReplacementCount, b => b.ReplacementCount + 1), ct);
        if (rows == 0)
            throw new InvalidOperationException($"تم تجاوز الحد الأقصى للاستبدال بهذا السبب ({max} مرات)");

        booking = await dbContext.Bookings
            .Include(b => b.Currency)
            .FirstAsync(b => b.Id == r.BookingId, ct);

        // تحرير العاملة القديمة
        var oldWorker = await dbContext.WorkerProfiles
            .FirstOrDefaultAsync(w => w.UserId == booking.WorkerId, ct);
        if (oldWorker is not null)
            oldWorker.IsAvailable = true;

        var newWorker = await ResolveNewWorkerAsync(r, booking, ct);

        booking.LastReplacementReason = r.Reason;
        booking.UpdatedAt = DateTime.UtcNow;

        // ══════════════════════════════════════════════════════════════════
        // الحالة 1: يومي/ساعي — وحدات ذرية مستقلة.
        // الحجز القديم يتقفل ويتم إنشاء حجز جديد بعمولته المستقلة.
        // ══════════════════════════════════════════════════════════════════
        if (booking.BookingType is BookingType.Daily or BookingType.Hourly)
        {
            booking.Status = BookingStatus.Replaced;

            var settings = await dbContext.AppSettings.ToListAsync(ct);
            var getPercent = (string key, int fallback) =>
            {
                var val = settings.FirstOrDefault(s => s.Key == key)?.Value;
                return int.TryParse(val, out var p) ? p : fallback;
            };

            var commissionPercent = booking.BookingType == BookingType.Daily
                ? getPercent("CommissionDailyPercent", 10)
                : getPercent("CommissionHourlyPercent", 10);

            var newTotal = booking.BookingType == BookingType.Daily
                ? newWorker.DailyRate * booking.Quantity
                : newWorker.HourlyRate * booking.Quantity;
            var dailyNewTotalInEgp = newTotal * newWorker.RateToEgp;
            var newCommissionInEgp = dailyNewTotalInEgp * commissionPercent / 100m;

            var replacementBooking = new Booking
            {
                HomeownerId = booking.HomeownerId,
                WorkerId = newWorker.WorkerId,
                OriginalWorkerId = newWorker.WorkerProfileId,
                ServiceType = booking.ServiceType,
                BookingType = booking.BookingType,
                Quantity = booking.Quantity,
                StartDate = DateTime.UtcNow,
                DailySalary = booking.BookingType == BookingType.Daily ? newWorker.DailyRate : 0,
                HourlySalary = booking.BookingType == BookingType.Hourly ? newWorker.HourlyRate : 0,
                TotalAmount = newTotal,
                CommissionAmount = newCommissionInEgp,
                CommissionType = CommissionType.OneTime,
                Status = BookingStatus.Pending,
                ReplacementCount = 0,
                CurrencyId = newWorker.CurrencyId,
                ReplacedFromBookingId = booking.Id,
                JobPostId = booking.JobPostId
            };

            dbContext.Bookings.Add(replacementBooking);
            await dbContext.SaveChangesAsync(ct);
            return Unit.Value;
        }

        // ══════════════════════════════════════════════════════════════════
        // الحالة 2: شهري (OneTime أو Subscription) — تناسب زمني موحد.
        // العمولة تُحسب نسبياً للأيام المتبقية فقط بسعر العاملة الجديدة.
        // ══════════════════════════════════════════════════════════════════

        var daysElapsed = Math.Clamp((int)(DateTime.UtcNow - booking.StartDate).TotalDays, 0, BillingPeriodDays);
        var daysRemaining = BillingPeriodDays - daysElapsed;
        var remainingRatio = daysRemaining / (decimal)BillingPeriodDays;

        var oldRateToEgp = booking.Currency?.RateToEgp ?? 1m;
        var oldTotalInEgp = booking.TotalAmount * oldRateToEgp;
        var monthlyNewTotalInEgp = newWorker.MonthlyRate * newWorker.RateToEgp;

        // النسبة المئوية للعمولة المتفق عليها أصلاً
        var commissionRatio = oldTotalInEgp > 0 ? booking.CommissionAmount / oldTotalInEgp : 0;

        // العمولة المستحقة عن الأيام اللي مضت (ثابتة، غير قابلة للاسترداد)
        var earnedCommission = booking.CommissionAmount * (1 - remainingRatio);

        // العمولة القديمة المخصصة للأيام المتبقية (التي سيتم استبدالها)
        var oldRemainingCommission = booking.CommissionAmount * remainingRatio;

        // العمولة الجديدة للأيام المتبقية بسعر العاملة الجديدة
        var newRemainingCommission = monthlyNewTotalInEgp * commissionRatio * remainingRatio;

        // المبلغ الواجب دفعه أو استرداده
        var diff = newRemainingCommission - oldRemainingCommission;

        // ══════════════════════════════════════════════════════════
        // استثناء: خطأ العاملة — المنصة تتحمل الفرق
        // ══════════════════════════════════════════════════════════
        if (r.Reason == ReplacementReason.WorkerFault)
        {
            // العمولة تبقى كما هي (المنصة تتحمل فرق السعر)
            booking.CommissionAmount = earnedCommission + oldRemainingCommission;
            booking.OutstandingAmount = 0;
        }
        else
        {
            // رغبة شخصية — العمولة الجديدة للفترة المتبقية
            booking.CommissionAmount = earnedCommission + newRemainingCommission;

            if (diff > 0 && booking.IsPaid)
            {
                // العاملة الجديدة أغلى — يحتاج دفع الفرق
                booking.OutstandingAmount = diff;
            }
            else
            {
                // نفس السعر أو أقل — لا دفع إضافي
                booking.OutstandingAmount = 0;
            }
        }

        // تحديث بيانات العاملة الجديدة
        booking.WorkerId = newWorker.WorkerId;
        booking.OriginalWorkerId = newWorker.WorkerProfileId;
        booking.MonthlySalary = newWorker.MonthlyRate;
        booking.CurrencyId = newWorker.CurrencyId;
        booking.TotalAmount = newWorker.MonthlyRate;

        // تعيين الحالة
        if (booking.OutstandingAmount > 0)
        {
            booking.Status = BookingStatus.ReplacementRequested;
        }
        else if (booking.IsPaid)
        {
            booking.Status = BookingStatus.Active;
        }
        else
        {
            booking.Status = BookingStatus.Pending;
        }

        // تعيين العاملة الجديدة كغير متاحة (لأننا تخطينا ConfirmWorker)
        var newWorkerProfile = await dbContext.WorkerProfiles
            .FirstOrDefaultAsync(w => w.UserId == newWorker.WorkerId, ct);
        if (newWorkerProfile is not null)
            newWorkerProfile.IsAvailable = false;

        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }

    private static string AppendNote(string? existing, string note) =>
        string.IsNullOrWhiteSpace(existing) ? note : $"{existing} | {note}";

    private sealed record NewWorkerInfo(
        string WorkerId, int WorkerProfileId, decimal DailyRate, decimal HourlyRate,
        decimal MonthlyRate, int CurrencyId, decimal RateToEgp);

    private async Task<NewWorkerInfo> ResolveNewWorkerAsync(
        RequestReplacementCommand r, Booking booking, CancellationToken ct)
    {
        if (r.ApplicationId.HasValue)
        {
            var app = await dbContext.JobApplications
                .Include(a => a.JobPost).ThenInclude(j => j.Currency)
                .FirstOrDefaultAsync(a => a.Id == r.ApplicationId && a.JobPostId == booking.JobPostId, ct)
                ?? throw new KeyNotFoundException("الطلب غير موجود");

            return new NewWorkerInfo(
                app.WorkerId, 0, app.JobPost.DailySalary, app.JobPost.HourlySalary,
                app.JobPost.MonthlySalary, app.JobPost.CurrencyId, app.JobPost.Currency.RateToEgp);
        }

        var newWorker = await dbContext.WorkerProfiles
            .Include(w => w.Currency)
            .FirstOrDefaultAsync(w => w.Id == r.NewWorkerId, ct)
            ?? throw new KeyNotFoundException("العاملة غير موجودة");

        return new NewWorkerInfo(
            newWorker.UserId, newWorker.Id, newWorker.DailyRate ?? 0, newWorker.HourlyRate ?? 0,
            newWorker.MonthlyRate ?? 0, newWorker.CurrencyId, newWorker.Currency.RateToEgp);
    }
}