using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Bookings.Common;
using MaidsAndNannies.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Admin.Queries.GetAllBookings;

public sealed class GetAllBookingsQueryHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<GetAllBookingsQuery, IReadOnlyList<AdminBookingListDto>>
{
    public async Task<IReadOnlyList<AdminBookingListDto>> Handle(GetAllBookingsQuery request, CancellationToken ct)
    {
        var settings = await dbContext.AppSettings.ToListAsync(ct);
        var settingsMaxFault = int.TryParse(settings.FirstOrDefault(s => s.Key == "MaxFaultReplacementCount")?.Value, out var smf) ? smf : 3;
        var settingsMaxPreference = int.TryParse(settings.FirstOrDefault(s => s.Key == "MaxPreferenceReplacementCount")?.Value, out var smp) ? smp : 1;

        var billingMode = settings.FirstOrDefault(s => s.Key == "CommissionBillingMode")?.Value ?? "CommissionOnly";

        var homeowners = await dbContext.HomeownerProfiles
            .Select(h => new { h.UserId, h.MaxFaultReplacementCount, h.MaxPreferenceReplacementCount })
            .ToListAsync(ct);
        var homeownersByUserId = homeowners.ToDictionary(h => h.UserId);

        var bookings = await dbContext.Bookings
            .Include(b => b.Homeowner)
            .Include(b => b.Worker)
            .Include(b => b.Currency)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

        return bookings.Select(b =>
        {
            homeownersByUserId.TryGetValue(b.HomeownerId, out var ho);

            var maxFault = ho?.MaxFaultReplacementCount ?? settingsMaxFault;
            var maxPreference = ho?.MaxPreferenceReplacementCount ?? settingsMaxPreference;
            var maxReplacement = Math.Max(maxFault, maxPreference);

            var rateToEgp = b.Currency?.RateToEgp ?? 1m;

            // المبلغ الإجمالي المستحق من صاحبة المنزل حسب الإعداد
            var workerFirstSalaryInEgp = b.BookingType switch
            {
                BookingType.Daily => b.DailySalary * b.Quantity * rateToEgp,
                BookingType.Hourly => b.HourlySalary * b.Quantity * rateToEgp,
                _ => b.MonthlySalary * rateToEgp
            };
            var paymentAmount = billingMode == "CommissionPlusSalary"
                ? b.CommissionAmount + workerFirstSalaryInEgp
                : b.CommissionAmount;

            return new AdminBookingListDto(
                b.Id,
                b.Worker.FullName,
                b.Homeowner.FullName,
                b.OriginalWorkerId ?? 0,
                b.ServiceType,
                b.BookingType,
                b.Quantity,
                b.Currency?.Code ?? "EGP",
                b.StartDate,
                b.MonthlySalary,
                b.DailySalary,
                b.HourlySalary,
                b.TotalAmount,
                b.TotalAmount * rateToEgp,
                b.CommissionAmount,
                b.Status,
                b.IsPaid,
                b.ReplacementCount,
                maxReplacement,
                b.CreatedAt,
                paymentAmount);
        }).ToList();
    }
}