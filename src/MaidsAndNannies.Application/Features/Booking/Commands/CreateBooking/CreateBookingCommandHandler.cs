using MaidsAndNannies.Application.Common.Interfaces;
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
        await using var transaction = await (dbContext ).Database.BeginTransactionAsync();
        try
        {
            var worker = await dbContext.WorkerProfiles
           .FirstOrDefaultAsync(w => w.Id == request.WorkerId, ct)
           ?? throw new KeyNotFoundException("العاملة غير موجودة");

            // تحديث ذري: نضع IsAvailable=false فقط إذا كانت true
            var rows = await dbContext.WorkerProfiles
                .Where(w => w.Id == request.WorkerId && w.IsAvailable)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(w => w.IsAvailable, false), ct);

            if (rows == 0)
                throw new InvalidOperationException("العاملة غير متاحة حالياً");

            var currency = await dbContext.Currencies
                .FirstOrDefaultAsync(c => c.Id == worker.CurrencyId, ct)
                ?? throw new KeyNotFoundException("العملة غير موجودة");

            var settings = await dbContext.AppSettings.ToListAsync(ct);

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

            decimal totalAmount = request.BookingType switch
            {
                BookingType.Daily => (worker.DailyRate ?? 0) * request.Quantity,
                BookingType.Hourly => (worker.HourlyRate ?? 0) * request.Quantity,
                BookingType.Monthly => request.MonthlySalary,
                _ => request.MonthlySalary
            };

            var totalInEgp = totalAmount * currency.RateToEgp;
            var commissionAmount = totalInEgp * commissionPercent / 100m;

            var commissionType = request.BookingType switch
            {
                BookingType.Monthly => request.CommissionType,
                _ => CommissionType.OneTime
            };

            var booking = new MaidsAndNannies.Domain.Entities.Booking
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
                ReplacementCount = 0,
                CurrencyId = worker.CurrencyId,
            };

            dbContext.Bookings.Add(booking);

            await dbContext.SaveChangesAsync(ct);

            if (commissionType == CommissionType.Subscription)
            {
                dbContext.Subscriptions.Add(new Domain.Entities.Subscription
                {
                    HomeownerId = request.HomeownerId,
                    BookingId = booking.Id,
                    PlanType = CommissionType.Subscription,
                    Amount = commissionAmount,
                    StartDate = request.StartDate.Date.ToUniversalTime(),
                    EndDate = request.StartDate.Date.ToUniversalTime().AddDays(30),
                    IsActive = true
                });
            }

            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync();
            return booking.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }
       
    }
}