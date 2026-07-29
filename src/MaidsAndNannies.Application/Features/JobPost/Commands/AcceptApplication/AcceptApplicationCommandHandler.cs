using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.JobPosts.Commands.AcceptApplication;

public sealed class AcceptApplicationCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<AcceptApplicationCommand, int>
{
    public async Task<int> Handle(AcceptApplicationCommand r, CancellationToken ct)
    {
        var post = await dbContext.JobPosts
            .Include(j => j.Applications)
            .Include(j => j.Currency)
            .FirstOrDefaultAsync(j => j.Id == r.PostId && j.HomeownerId == r.HomeownerId, ct)
            ?? throw new KeyNotFoundException("الإعلان غير موجود");

        if (post.PostStatus != JobPostStatus.Approved)
            throw new InvalidOperationException("الإعلان غير معتمد بعد");

        var app = post.Applications.FirstOrDefault(a => a.Id == r.AppId)
            ?? throw new KeyNotFoundException("الطلب غير موجود");
        if (app.Status != ApplicationStatus.Pending)
            throw new InvalidOperationException("الطلب لم يعد قيد الانتظار");

        // منع سباق التزامن: التأكد بعد التحديث المباشر
        var rows = await dbContext.JobApplications
            .Where(a => a.Id == r.AppId && a.Status == ApplicationStatus.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(a => a.Status, ApplicationStatus.Accepted), ct);
        if (rows == 0)
            throw new InvalidOperationException("الطلب لم يعد قيد الانتظار");

        decimal totalAmount = post.BookingType switch
        {
            BookingType.Daily => post.DailySalary * post.Quantity,
            BookingType.Hourly => post.HourlySalary * post.Quantity,
            BookingType.Monthly => post.MonthlySalary,
            _ => post.MonthlySalary
        };

        var settings = await dbContext.AppSettings.ToListAsync(ct);
        var getPercent = (string key, int fallback) =>
        {
            var val = settings.FirstOrDefault(s => s.Key == key)?.Value;
            return int.TryParse(val, out var p) ? p : fallback;
        };

        var commissionPercent = post.BookingType switch
        {
            BookingType.Daily => getPercent("CommissionDailyPercent", 10),
            BookingType.Hourly => getPercent("CommissionHourlyPercent", 10),
            BookingType.Monthly => post.CommissionType == CommissionType.OneTime
                ? getPercent("CommissionMonthlyOneTimePercent", 10)
                : getPercent("CommissionMonthlySubscriptionPercent", 10),
            _ => 10
        };

        var totalInEgp = totalAmount * post.Currency.RateToEgp;
        var commissionAmount = totalInEgp * commissionPercent / 100m;

        var commissionType = post.BookingType switch
        {
            BookingType.Monthly => post.CommissionType,
            _ => CommissionType.OneTime
        };

        var booking = new Booking
        {
            HomeownerId = r.HomeownerId,
            WorkerId = app.WorkerId,
            ServiceType = post.Specialization,
            BookingType = post.BookingType,
            Quantity = post.Quantity,
            StartDate = post.StartDate,
            MonthlySalary = post.MonthlySalary,
            DailySalary = post.DailySalary,
            HourlySalary = post.HourlySalary,
            TotalAmount = totalAmount,
            CommissionAmount = commissionAmount,
            CommissionType = commissionType,
            Status = BookingStatus.Pending,
            JobPostId = r.PostId,
            ReplacementCount = 0,
            CurrencyId = post.CurrencyId,
        };
        dbContext.Bookings.Add(booking);

        if (commissionType == CommissionType.Subscription)
        {
            dbContext.Subscriptions.Add(new Domain.Entities.Subscription
            {
                HomeownerId = r.HomeownerId,
                BookingId = booking.Id,
                PlanType = CommissionType.Subscription,
                Amount = commissionAmount,
                StartDate = post.StartDate,
                EndDate = post.StartDate.AddDays(30),
                IsActive = true
            });
        }

        await dbContext.SaveChangesAsync(ct);
        return booking.Id;
    }
}