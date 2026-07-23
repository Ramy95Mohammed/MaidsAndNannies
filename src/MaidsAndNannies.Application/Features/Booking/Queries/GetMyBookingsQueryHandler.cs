using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Bookings.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Bookings.Queries.GetMyBookings;

public sealed class GetMyBookingsQueryHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<GetMyBookingsQuery, IReadOnlyList<BookingListDto>>
{
    public async Task<IReadOnlyList<BookingListDto>> Handle(GetMyBookingsQuery request, CancellationToken ct)
    {
        return await dbContext.Bookings
            .Include(b => b.Worker)
            .Where(b => b.HomeownerId == request.UserId)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BookingListDto(
                b.Id,
                b.Worker.FullName,
                0, // WorkerProfile Id
                b.ServiceType,
                b.StartDate,
                b.MonthlySalary,
                b.CommissionAmount,
                b.Status,
                b.IsPaid,
                b.ReplacementCount,
                b.CreatedAt))
            .ToListAsync(ct);
    }
}