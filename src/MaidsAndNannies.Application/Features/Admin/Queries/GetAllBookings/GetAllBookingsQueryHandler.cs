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
        var bookings = await dbContext.Bookings
            .Include(b => b.Homeowner)
            .Include(b => b.Worker)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new AdminBookingListDto(
                b.Id,
                b.Worker.FullName,
                b.Homeowner.FullName,
                0,
                b.ServiceType,
                b.StartDate,
                b.MonthlySalary,
                b.CommissionAmount,
                b.Status,
                b.IsPaid,
                b.ReplacementCount,                
                b.CreatedAt))
            .ToListAsync(ct);
        return bookings;
    }
}