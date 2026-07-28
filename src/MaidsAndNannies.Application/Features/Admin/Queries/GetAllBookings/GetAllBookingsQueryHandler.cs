using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Bookings.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Admin.Queries.GetAllBookings;

public sealed class GetAllBookingsQueryHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<GetAllBookingsQuery, IReadOnlyList<AdminBookingListDto>>
{
    public async Task<IReadOnlyList<AdminBookingListDto>> Handle(GetAllBookingsQuery request, CancellationToken ct)
    {
        // نفس ملاحظة GetBookingByIdQueryHandler: الحد بقى منفصل حسب سبب الاستبدال
        var replacementSettings = await dbContext.AppSettings
            .Where(s => s.Key == "MaxFaultReplacementCount" || s.Key == "MaxPreferenceReplacementCount")
            .ToListAsync(ct);
        var maxFault = int.TryParse(replacementSettings.FirstOrDefault(s => s.Key == "MaxFaultReplacementCount")?.Value, out var mf) ? mf : 3;
        var maxPreference = int.TryParse(replacementSettings.FirstOrDefault(s => s.Key == "MaxPreferenceReplacementCount")?.Value, out var mp) ? mp : 1;
        var maxReplacement = Math.Max(maxFault, maxPreference);

        return await dbContext.Bookings
            .Include(b => b.Homeowner)
            .Include(b => b.Worker)
            .Include(b => b.Currency)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new AdminBookingListDto(
                b.Id,
                b.Worker.FullName,
                b.Homeowner.FullName,
                b.OriginalWorkerId ?? 0,
                b.ServiceType,
                b.BookingType,
                b.Quantity,
                b.Currency != null ? b.Currency.Code : "EGP",
                b.StartDate,
                b.MonthlySalary,
                b.DailySalary,
                b.HourlySalary,
                b.TotalAmount,
                b.TotalAmount * (b.Currency != null ? b.Currency.RateToEgp : 1m),
                b.CommissionAmount,
                b.Status,
                b.IsPaid,
                b.ReplacementCount,
                maxReplacement,
                b.CreatedAt))
            .ToListAsync(ct);
    }
}