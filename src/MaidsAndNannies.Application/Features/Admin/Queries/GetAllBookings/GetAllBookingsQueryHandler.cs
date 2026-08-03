using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Bookings.Common;
using MaidsAndNannies.Application.Features.Worker.Common;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Admin.Queries.GetAllBookings;

public sealed class GetAllBookingsQueryHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<GetAllBookingsQuery, PagedResult<AdminBookingListDto>>
{
    public async Task<PagedResult<AdminBookingListDto>> Handle(GetAllBookingsQuery request, CancellationToken ct)
    {
        if (request.Page < 1) request = request with { Page = 1 };
        if (request.PageSize < 1 || request.PageSize > 50) request = request with { PageSize = 10 };

        var settings = await dbContext.AppSettings.ToListAsync(ct);
        var settingsMaxFault = int.TryParse(settings.FirstOrDefault(s => s.Key == "MaxFaultReplacementCount")?.Value, out var smf) ? smf : 3;
        var settingsMaxPreference = int.TryParse(settings.FirstOrDefault(s => s.Key == "MaxPreferenceReplacementCount")?.Value, out var smp) ? smp : 1;

        var billingMode = settings.FirstOrDefault(s => s.Key == "CommissionBillingMode")?.Value ?? "CommissionOnly";

        var homeowners = await dbContext.HomeownerProfiles
            .Select(h => new { h.UserId, h.MaxFaultReplacementCount, h.MaxPreferenceReplacementCount })
            .ToListAsync(ct);
        var homeownersByUserId = homeowners.ToDictionary(h => h.UserId);

        var query = dbContext.Bookings.AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(b => b.Status == (BookingStatus)request.Status.Value);
        if (request.IsPaid.HasValue)
            query = query.Where(b => b.IsPaid == request.IsPaid.Value);
        if (request.FromDate.HasValue)
            query = query.Where(b => b.StartDate >= request.FromDate.Value.Date);
        if (request.ToDate.HasValue)
            query = query.Where(b => b.StartDate < request.ToDate.Value.Date.AddDays(1));
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(b => b.Worker.FullName.Contains(search) || b.Homeowner.FullName.Contains(search));
        }

        var totalCount = await query.CountAsync(ct);

        var bookings = await query
            .Include(b => b.Homeowner)
            .Include(b => b.Worker)
            .Include(b => b.Currency)
            .OrderByDescending(b => b.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = bookings.Select(b =>
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

        return new PagedResult<AdminBookingListDto>(items, totalCount, request.Page, request.PageSize);
    }
}