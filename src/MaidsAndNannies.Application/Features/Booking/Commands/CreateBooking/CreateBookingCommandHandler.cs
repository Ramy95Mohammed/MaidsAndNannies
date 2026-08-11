using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Booking.Common;
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
    ICalculateBookingCommissionData calculateBookingCommissionData,
    INotificationService notifications)
    : IRequestHandler<CreateBookingCommand, BookingDetailDto>
{
    public async Task<BookingDetailDto> Handle(
    CreateBookingCommand request,
    CancellationToken ct)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();

        BookingDetailDto? result = null;

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await dbContext.Database.BeginTransactionAsync(ct);

            try
            {
                var worker = await dbContext.WorkerProfiles
                    .FirstOrDefaultAsync(
                        w => w.Id == request.WorkerId,
                        ct)
                    ?? throw new KeyNotFoundException("العاملة غير موجودة");

                var currency = await dbContext.Currencies
                    .FirstOrDefaultAsync(
                        c => c.Id == worker.CurrencyId,
                        ct)
                    ?? throw new KeyNotFoundException("العملة غير موجودة");

                var workerCalculationsVars =
                    new HomeownerOrWorkerCalculationsVars
                    {
                        DailyRate = worker.DailyRate,
                        HourlyRate = worker.HourlyRate,
                        MonthlyRate = worker.MonthlyRate
                    };

                var bookingRequest = new BookingOrJobPostRequestVars
                {
                    BookingType = request.BookingType,
                    CommissionType = request.CommissionType,
                    MonthlySalary = request.MonthlySalary,
                    Quantity = request.Quantity
                };

                var bookingCalculationsReturnValue =
                    await calculateBookingCommissionData.Calc(
                        bookingRequest,
                        workerCalculationsVars,
                        currency,
                        ct);

                int bookingId = 0;

                if (!request.CalcOnly)
                {
                    var booking =
                        new MaidsAndNannies.Domain.Entities.Booking
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
                            TotalAmount =
                                bookingCalculationsReturnValue.TotalAmount,
                            CommissionAmount =
                                bookingCalculationsReturnValue.CommissionAmount,
                            CommissionType =
                                bookingCalculationsReturnValue.CommissionType,
                            Status = BookingStatus.Pending,
                            ReplacementCount = 0,
                            CurrencyId = worker.CurrencyId
                        };

                    var homeownerProfile =
                        await dbContext.HomeownerProfiles
                            .FirstOrDefaultAsync(
                                h => h.UserId == request.HomeownerId,
                                ct);

                    if (homeownerProfile is not null &&
                        homeownerProfile.TermsAcceptedAt is null)
                    {
                        homeownerProfile.TermsAcceptedAt = DateTime.UtcNow;
                        homeownerProfile.TermsAcceptedVersion = "1.0";
                    }

                    dbContext.Bookings.Add(booking);

                    await dbContext.SaveChangesAsync(ct);

                    if (bookingCalculationsReturnValue.CommissionType ==
                        CommissionType.Subscription)
                    {
                        dbContext.Subscriptions.Add(
                            new Domain.Entities.Subscription
                            {
                                HomeownerId = request.HomeownerId,
                                BookingId = booking.Id,
                                PlanType = CommissionType.Subscription,
                                Amount =
                                    bookingCalculationsReturnValue.CommissionAmount,
                                StartDate =
                                    request.StartDate.Date.ToUniversalTime(),
                                EndDate =
                                    request.StartDate.Date
                                        .ToUniversalTime()
                                        .AddDays(30),
                                IsActive = true
                            });
                    }

                    worker.IsAvailable = false;

                    await dbContext.SaveChangesAsync(ct);

                    await transaction.CommitAsync(ct);

                    bookingId = booking.Id;
                }

                result = new BookingDetailDto(bookingId, "", "", "", null, "", "", null, null, null, null, Specialization.Childcare, BookingType.Daily, 0, "", DateTime.Now, null, 0, 0, 0, bookingCalculationsReturnValue.TotalAmount, bookingCalculationsReturnValue.TotalInEgp, bookingCalculationsReturnValue.CommissionAmount, CommissionType.OneTime, BookingStatus.Pending, false, 0, 0, null, DateTime.Now, null, 0, 0, 0, bookingCalculationsReturnValue.PaymentAmount, true, false);

                // مهم: الـ notification خارج الـ transaction
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        });

        if (!request.CalcOnly && result is not null)
        {
            await notifications.NotifyAdminsAsync(
                NotificationType.BookingCreated,
                "NOTIF.BOOKING_CREATED",
                new { BookingId = result.Id },
                ct);
        }

        return result!;
    }

}