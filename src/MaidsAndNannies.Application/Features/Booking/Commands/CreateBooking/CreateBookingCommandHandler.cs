using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.CreateBooking;

public sealed class CreateBookingCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<CreateBookingCommand, int>
{
    public async Task<int> Handle(CreateBookingCommand request, CancellationToken ct)
    {
        var worker = await dbContext.WorkerProfiles
            .FirstOrDefaultAsync(w => w.Id == request.WorkerId, ct)
            ?? throw new KeyNotFoundException("العاملة غير موجودة");

        var currency = await dbContext.Currencies
            .FirstOrDefaultAsync(c => c.Id == worker.CurrencyId, ct)
            ?? throw new KeyNotFoundException("العملة غير موجودة");

        var settings = await dbContext.AppSettings
    .ToListAsync(ct);

        var getPercent = (string key, int fallback) =>
        {
            var val = settings.FirstOrDefault(s => s.Key == key)?.Value;
            return int.TryParse(val, out var p) ? p : fallback;
        };

        var commissionPercent = request.BookingType switch
        {
            BookingType.Daily => getPercent("CommissionDailyPercent", 10),
            BookingType.Hourly => getPercent("CommissionHourlyPercent", 10),
            BookingType.Monthly => request.CommissionType == CommissionType.OneTime
                ? getPercent("CommissionMonthlyOneTimePercent", 10)
                : getPercent("CommissionMonthlySubscriptionPercent", 10),
            _ => 10
        };
        

        // حساب الإجمالي
        decimal totalAmount = request.BookingType switch
        {
            BookingType.Daily => (worker.DailyRate ?? 0) * request.Quantity,
            BookingType.Hourly => (worker.HourlyRate ?? 0) * request.Quantity,
            BookingType.Monthly => request.MonthlySalary,
            _ => request.MonthlySalary
        };

        // تحويل إلى EGP لحساب العمولة
        var totalInEgp = totalAmount * currency.RateToEgp;

        var commissionAmount = totalInEgp * commissionPercent / 100m;
        //var commissionAmount = totalInEgp * 0.1m;

        // لو يومي أو ساعي، العمولة OneTime
        var commissionType = request.BookingType switch
        {
            BookingType.Monthly => request.CommissionType,
            _ => CommissionType.OneTime
        };

        var booking = new Booking
        {
            HomeownerId = request.HomeownerId,
            WorkerId = worker.UserId,
            OriginalWorkerId = worker.Id,
            ServiceType = request.ServiceType,
            BookingType = request.BookingType,
            Quantity = request.Quantity,
            StartDate = request.StartDate,
            MonthlySalary = request.MonthlySalary,
            DailySalary = request.DailySalary,
            HourlySalary = request.HourlySalary,
            TotalAmount = totalAmount,
            CommissionAmount = commissionAmount,
            CommissionType = commissionType,
            Status = BookingStatus.Pending,
            ReplacementCount = 0
        };

        dbContext.Bookings.Add(booking);

        if (commissionType == CommissionType.Subscription)
        {
            dbContext.Subscriptions.Add(new Domain.Entities.Subscription
            {
                HomeownerId = request.HomeownerId,
                PlanType = CommissionType.Subscription,
                Amount = commissionAmount,
                StartDate = request.StartDate,
                EndDate = request.StartDate.AddDays(30),
                IsActive = true
            });
        }

        await dbContext.SaveChangesAsync(ct);
        return booking.Id;
    }
}