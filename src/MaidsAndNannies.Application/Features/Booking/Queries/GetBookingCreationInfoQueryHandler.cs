using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Bookings.Common;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Booking.Queries
{
    public class GetBookingCreationInfoQueryHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<GetBookingCreationInfoQuery, BookingDetailDto>
    {
        public async Task<BookingDetailDto> Handle(GetBookingCreationInfoQuery request, CancellationToken ct)
        {
            await using var transaction = await(dbContext).Database.BeginTransactionAsync();
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

                var booking = new BookingDetailDto(0, "", "", null, "", "", null, null, null,
                    null, Specialization.Childcare, BookingType.Daily, 0, "", DateTime.Now, null,
                    0, 0, 0, totalAmount, totalInEgp, commissionAmount, CommissionType.OneTime, BookingStatus.Pending,
                    false, 0, 0, null,DateTime.Now, null, 0, 0, 0);
                               
                await transaction.CommitAsync();
                return booking;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
