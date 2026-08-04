using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Bookings.Common;
using MaidsAndNannies.Application.Features.Notifications;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.CreateBooking;

public sealed class CreateBookingCommandHandler(
    IApplicationDbContext dbContext ,    
    INotificationService notifications)
    : IRequestHandler<CreateBookingCommand, BookingDetailDto>
{
    public async Task<BookingDetailDto> Handle(CreateBookingCommand request, CancellationToken ct)
    {
        await using var transaction = await (dbContext ).Database.BeginTransactionAsync();
        try
        {
            var worker = await dbContext.WorkerProfiles
               .FirstOrDefaultAsync(w => w.Id == request.WorkerId, ct)
               ?? throw new KeyNotFoundException("العاملة غير موجودة");



            var currency = await dbContext.Currencies
                .FirstOrDefaultAsync(c => c.Id == worker.CurrencyId, ct)
                ?? throw new KeyNotFoundException("العملة غير موجودة");

            var settings = await dbContext.AppSettings.ToListAsync(ct);

            var getPercent = (string key, int fallback) =>
            {
                var val = settings.FirstOrDefault(s => s.Key == key)?.Value;
                return int.TryParse(val, out var p) ? p : fallback;
            };

            var monthlyWorkingDays = getPercent("MonthlyWorkingDaysPerMonth", 26);
            if (monthlyWorkingDays < 1) monthlyWorkingDays = 26;

            var monthlyTotal = request.Quantity > 0
               ? request.MonthlySalary / monthlyWorkingDays * request.Quantity
               : request.MonthlySalary;

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
                BookingType.Monthly => monthlyTotal,
                _ => monthlyTotal
            };

            var totalInEgp = totalAmount * currency.RateToEgp;
            var commissionAmount = totalInEgp * commissionPercent / 100m;

            // المبلغ الإجمالي المطلوب عند الدفع حسب الإعداد
            var billingMode = settings.FirstOrDefault(s => s.Key == "CommissionBillingMode")?.Value ?? "CommissionOnly";

            var workerFirstSalaryInEgp = request.BookingType switch
            {
                BookingType.Daily => (worker.DailyRate ?? 0) * request.Quantity * currency.RateToEgp,
                BookingType.Hourly => (worker.HourlyRate ?? 0) * request.Quantity * currency.RateToEgp,
                _ => monthlyTotal * currency.RateToEgp
            };

            var paymentAmount = billingMode == "CommissionPlusSalary"
                ? commissionAmount + workerFirstSalaryInEgp
                : commissionAmount;

            var commissionType = request.BookingType switch
            {
                BookingType.Monthly => request.CommissionType,
                _ => CommissionType.OneTime
            };


            int bookinId = 0;

            if (request.CalcOnly == false)
            {
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



                var homeownerProfile = await dbContext.HomeownerProfiles
                      .FirstOrDefaultAsync(h => h.UserId == request.HomeownerId, ct);

                if (homeownerProfile is not null && homeownerProfile.TermsAcceptedAt is null)
                {
                    homeownerProfile.TermsAcceptedAt = DateTime.UtcNow;
                    homeownerProfile.TermsAcceptedVersion = "1.0";
                }

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

                bookinId = booking.Id;

                await notifications.NotifyAdminsAsync(NotificationType.BookingCreated, "NOTIF.BOOKING_CREATED",
                  new { BookingId = booking.Id }, ct);
            }


            var bookingDetailDto = new BookingDetailDto(bookinId, "", "", null, "", "", null, null, null,
               null, Specialization.Childcare, BookingType.Daily, 0, "", DateTime.Now, null,
               0, 0, 0, totalAmount, totalInEgp, commissionAmount, CommissionType.OneTime, BookingStatus.Pending,
               false, 0, 0, null, DateTime.Now, null, 0, 0, 0, paymentAmount, true, false);

            return bookingDetailDto;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }
       
    }
}