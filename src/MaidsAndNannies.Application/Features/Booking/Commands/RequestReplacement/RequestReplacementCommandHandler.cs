using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Notifications;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.RequestReplacement;

public sealed class RequestReplacementCommandHandler(
    IApplicationDbContext dbContext ,
     INotificationService notifications)
    : IRequestHandler<RequestReplacementCommand, Unit>
{
    private const int BillingPeriodDays = 30;

    public async Task<Unit> Handle(RequestReplacementCommand r, CancellationToken ct)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            var booking = await dbContext.Bookings
        .Include(b => b.Currency)
        .FirstOrDefaultAsync(b => b.Id == r.BookingId && b.HomeownerId == r.HomeownerId, ct)
        ?? throw new KeyNotFoundException("الحجز غير موجود");

            // ── حد أقصى للاستبدال: قيمة مخصصة لصاحبة المنزل إن وُجدت، وإلا من الإعدادات ──
            var maxKey = r.Reason == ReplacementReason.WorkerFault
                ? "MaxFaultReplacementCount"
                : "MaxPreferenceReplacementCount";
            var homeowner = await dbContext.HomeownerProfiles
                .FirstOrDefaultAsync(h => h.UserId == booking.HomeownerId, ct);
            var maxSetting = await dbContext.AppSettings.FirstOrDefaultAsync(s => s.Key == maxKey, ct);

            var max = r.Reason == ReplacementReason.WorkerFault
                ? homeowner?.MaxFaultReplacementCount
                    ?? (int.TryParse(maxSetting?.Value, out var mf) ? mf : 3)
                : homeowner?.MaxPreferenceReplacementCount
                    ?? (int.TryParse(maxSetting?.Value, out var mp) ? mp : 1);

            booking = await dbContext.Bookings
                .Include(b => b.Currency)
                .FirstAsync(b => b.Id == r.BookingId, ct);

            // ── منع تجاوز الحد الأقصى للاستبدال ──
            if (booking.ReplacementCount >= max)
                throw new InvalidOperationException(
                    r.Reason == ReplacementReason.WorkerFault
                        ? "تم استنفاد عدد استبدالات خطأ العاملة المسموح به"
                        : "تم استنفاد عدد الاستبدالات المسموح به");

            // تحرير العاملة القديمة
            var oldWorker = await dbContext.WorkerProfiles
                .FirstOrDefaultAsync(w => w.UserId == booking.WorkerId, ct);
            if (oldWorker is not null)
                oldWorker.IsAvailable = true;

            var newWorker = await ResolveNewWorkerAsync(r, booking, ct);

            // إشعارات الاستبدال (تشمل مساري اليومي/الساعي والشهري — old أولاً قبل استبدال WorkerId)
            await notifications.NotifyAsync(booking.WorkerId, NotificationType.ReplacedWorker, "NOTIF.REPLACED_WORKER",
                new { BookingId = booking.Id }, ct);
            await notifications.NotifyAsync(newWorker.WorkerId, NotificationType.ReplacementAssigned, "NOTIF.REPLACEMENT_ASSIGNED",
                new { BookingId = booking.Id }, ct);


            booking.LastReplacementReason = r.Reason;
            booking.UpdatedAt = DateTime.UtcNow;

            // ══════════════════════════════════════════════════════════════════
            // الحالة 1: يومي/ساعي — حجز جديد مستقل بعمولته، محسوب على الوحدات المتبقية فقط
            // (وليس الكمية الأصلية كاملة) عشان صاحبة المنزل متدفعش عن أيام/ساعات
            // اتصرفت بالفعل مع العاملة القديمة.
            // ══════════════════════════════════════════════════════════════════
            if (booking.BookingType is BookingType.Daily or BookingType.Hourly)
            {
                // عدد الوحدات (أيام أو ساعات) اللي فاتت من الحجز الأصلي منذ بدايته
                var elapsedUnits = booking.BookingType == BookingType.Daily
                    ? (int)(DateTime.UtcNow - booking.StartDate).TotalDays
                    : (int)(DateTime.UtcNow - booking.StartDate).TotalHours;
                elapsedUnits = Math.Clamp(elapsedUnits, 0, booking.Quantity);
                var remainingQuantity = booking.Quantity - elapsedUnits;

                if (remainingQuantity <= 0)
                    throw new InvalidOperationException(
                        "الحجز الأصلي انتهت مدته بالفعل، لا يوجد وقت متبقٍ ليتم استبداله");

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
                    ? newWorker.DailyRate * remainingQuantity
                    : newWorker.HourlyRate * remainingQuantity;
                var dailyNewTotalInEgp = newTotal * newWorker.RateToEgp;
                var newCommissionInEgp = dailyNewTotalInEgp * commissionPercent / 100m;

                var replacementBooking = new Domain.Entities.Booking
                {
                    HomeownerId = booking.HomeownerId,
                    WorkerId = newWorker.WorkerId,
                    OriginalWorkerId = newWorker.WorkerProfileId,
                    ServiceType = booking.ServiceType,
                    BookingType = booking.BookingType,
                    Quantity = remainingQuantity,
                    StartDate = DateTime.UtcNow,
                    DailySalary = booking.BookingType == BookingType.Daily ? newWorker.DailyRate : 0,
                    HourlySalary = booking.BookingType == BookingType.Hourly ? newWorker.HourlyRate : 0,
                    TotalAmount = newTotal,
                    CommissionAmount = newCommissionInEgp,
                    CommissionType = CommissionType.OneTime,
                    Status = BookingStatus.Pending,
                    ReplacementCount = booking.ReplacementCount + 1,
                    CurrencyId = newWorker.CurrencyId,
                    ReplacedFromBookingId = booking.Id,
                    JobPostId = booking.JobPostId
                };

                dbContext.Bookings.Add(replacementBooking);
                await dbContext.SaveChangesAsync(ct);
                await transaction.CommitAsync();
                return Unit.Value;
            }

            // ══════════════════════════════════════════════════════════════════
            // الحالة 2: شهري — عمولة متناسبة مع الأيام المتبقية بسعر العاملة الجديدة
            // ══════════════════════════════════════════════════════════════════
            var daysElapsed = Math.Clamp((int)(DateTime.UtcNow - booking.StartDate).TotalDays, 0, BillingPeriodDays);
            var daysRemaining = BillingPeriodDays - daysElapsed;
            var remainingRatio = daysRemaining / (decimal)BillingPeriodDays;

            var oldRateToEgp = booking.Currency?.RateToEgp ?? 1m;
            var oldTotalInEgp = booking.TotalAmount * oldRateToEgp;
            var monthlyNewTotalInEgp = newWorker.MonthlyRate * newWorker.RateToEgp;

            // النسبة المئوية للعمولة المتفق عليها أصلاً
            var commissionRatio = oldTotalInEgp > 0 ? booking.CommissionAmount / oldTotalInEgp : 0;

            // العمولة المستحقة عن الأيام الماضية (ثابتة، غير قابلة للاسترداد)
            var earnedCommission = booking.CommissionAmount * (1 - remainingRatio);
            // العمولة القديمة المخصصة للأيام المتبقية
            var oldRemainingCommission = booking.CommissionAmount * remainingRatio;
            // العمولة الجديدة للأيام المتبقية بسعر العاملة الجديدة
            var newRemainingCommission = monthlyNewTotalInEgp * commissionRatio * remainingRatio;
            // المبلغ الواجب دفعه أو استرداده
            var diff = newRemainingCommission - oldRemainingCommission;

            // استثناء: خطأ العاملة — المنصة تتحمل الفرق
            if (r.Reason == ReplacementReason.WorkerFault)
            {
                booking.CommissionAmount = earnedCommission + oldRemainingCommission;
                booking.OutstandingAmount = 0;
            }
            else
            {
                booking.CommissionAmount = earnedCommission + newRemainingCommission;
                booking.OutstandingAmount = diff > 0 && booking.IsPaid ? diff : 0;
            }

            // تحديث الاشتراك الشهري إن وجد
            if (booking.CommissionType == CommissionType.Subscription)
            {
                var subscription = await dbContext.Subscriptions
                    .FirstOrDefaultAsync(s => s.BookingId == booking.Id && s.IsActive, ct);

                if (subscription is not null)
                {
                    var newTotalCommission = earnedCommission + newRemainingCommission;
                    subscription.Amount = newTotalCommission;
                    subscription.UpdatedAt = DateTime.UtcNow;

                    // إذا كان الفرق موجباً ولم يُدفع بعد، الاشتراك ينتظر
                    if (booking.OutstandingAmount > 0)
                        subscription.IsActive = false;
                }
            }

            // تحديث بيانات العاملة الجديدة + ترقيم الاستبدال
            booking.WorkerId = newWorker.WorkerId;
            booking.OriginalWorkerId = newWorker.WorkerProfileId;
            booking.MonthlySalary = newWorker.MonthlyRate;
            booking.CurrencyId = newWorker.CurrencyId;
            booking.TotalAmount = newWorker.MonthlyRate;
            booking.ReplacementCount++;

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

            // تعيين العاملة الجديدة كغير متاحة
            var newWorkerProfile = await dbContext.WorkerProfiles
                .FirstOrDefaultAsync(w => w.UserId == newWorker.WorkerId, ct);
            if (newWorkerProfile is not null)
                newWorkerProfile.IsAvailable = false;

            await dbContext.SaveChangesAsync(ct);

            await transaction.CommitAsync();

            return Unit.Value;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }

    }

    private sealed record NewWorkerInfo(
        string WorkerId, int WorkerProfileId, decimal DailyRate, decimal HourlyRate,
        decimal MonthlyRate, int CurrencyId, decimal RateToEgp);

    private async Task<NewWorkerInfo> ResolveNewWorkerAsync(
        RequestReplacementCommand r, Domain.Entities.Booking booking, CancellationToken ct)
    {
        if (r.ApplicationId.HasValue)
        {
            var app = await dbContext.JobApplications
                .Include(a => a.JobPost).ThenInclude(j => j.Currency)
                .FirstOrDefaultAsync(a => a.Id == r.ApplicationId && a.JobPostId == booking.JobPostId, ct)
                ?? throw new KeyNotFoundException("الطلب غير موجود");

            // ترقية حالة الطلب إلى "مقبول" حتى لا يظل ظاهراً في قوائم المتقدمين
            var rows = await dbContext.JobApplications
                .Where(a => a.Id == r.ApplicationId && a.Status == ApplicationStatus.Pending)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(a => a.Status, ApplicationStatus.Accepted), ct);
            if (rows == 0)
                throw new InvalidOperationException("الطلب لم يعد قيد الانتظار");

            var workerId = (await dbContext.WorkerProfiles.FirstOrDefaultAsync(w => w.UserId == app.WorkerId))?.Id;
            if (workerId == null)
                throw new KeyNotFoundException("عاملة غير موجودة");

            return new NewWorkerInfo(
                app.WorkerId, workerId ?? 0, app.JobPost.DailySalary, app.JobPost.HourlySalary,
                app.JobPost.MonthlySalary, app.JobPost.CurrencyId, app.JobPost.Currency.RateToEgp);
        }

        var newWorker = await dbContext.WorkerProfiles
            .Include(w => w.Currency)
            .FirstOrDefaultAsync(w => w.Id == r.NewWorkerId, ct)
            ?? throw new KeyNotFoundException("العاملة غير موجودة");

        // الحجز ناتج من إعلان وظيفة: الراتب ثابت من الإعلان بغض النظر عن العاملة الجديدة،
        // فلا فرق في العمولة عند الاستبدال ولو برغبة صاحبة المنزل
        if (booking.JobPostId.HasValue)
        {
            var post = await dbContext.JobPosts
                .Include(j => j.Currency)
                .FirstOrDefaultAsync(j => j.Id == booking.JobPostId, ct);
            if (post is not null)
                return new NewWorkerInfo(
                    newWorker.UserId, newWorker.Id, post.DailySalary, post.HourlySalary,
                    post.MonthlySalary, post.CurrencyId, post.Currency.RateToEgp);
        }

        return new NewWorkerInfo(
            newWorker.UserId, newWorker.Id, newWorker.DailyRate ?? 0, newWorker.HourlyRate ?? 0,
            newWorker.MonthlyRate ?? 0, newWorker.CurrencyId, newWorker.Currency.RateToEgp);
    }
}