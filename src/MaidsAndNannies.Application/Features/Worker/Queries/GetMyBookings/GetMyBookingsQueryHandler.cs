using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Worker.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Worker.Queries.GetMyBookings;

public sealed class GetMyBookingsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetMyBookingsQuery, IReadOnlyList<MyBookingDto>>
{
    public async Task<IReadOnlyList<MyBookingDto>> Handle(GetMyBookingsQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Bookings
            .Include(b => b.Homeowner)
            .Where(b => b.WorkerId == request.UserId)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new MyBookingDto(
                b.Id,
                b.Homeowner.FullName,
                b.ServiceType,
                b.StartDate,
                b.EndDate,
                b.MonthlySalary,
                b.CommissionAmount,
                b.CommissionType,
                b.Status,
                b.IsPaid,
                b.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
