using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Bookings.Common;
using MaidsAndNannies.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MaidsAndNannies.Application.Features.Admin.Queries.GetAllBookings;

public sealed class GetAllBookingsQueryHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<GetAllBookingsQuery, IReadOnlyList<AdminBookingListDto>>
{
    public async Task<IReadOnlyList<AdminBookingListDto>> Handle(GetAllBookingsQuery request, CancellationToken ct)
    {

        var bookingList = await dbContext.Bookings
             .Include(b => b.Homeowner)
            .Include(b => b.Worker)
            .OrderByDescending(b => b.CreatedAt)
           .OrderByDescending(b => b.CreatedAt).Select(b => new
           {
               b.Id,
               WorkerFullName = b.Worker.FullName,
               HomeownerFullName = b.Homeowner.FullName,
               b.WorkerId,
               b.ServiceType,
               b.BookingType,
               b.Quantity,
               b.StartDate,
               b.MonthlySalary,
               b.DailySalary,
               b.HourlySalary,
               b.TotalAmount,
               b.CommissionAmount,
               b.Status,
               b.IsPaid,
               b.ReplacementCount,
               b.CreatedAt
           })
            .ToListAsync(ct);

        var workerProfiles = await dbContext.WorkerProfiles.Where(p => bookingList.Select(b => b.WorkerId).Contains(p.UserId))
           .Select(w => new WorkerProfile
           {
               Id = w.Id,
               UserId = w.UserId,
               Currency = new Domain.Entities.Currency
               {
                   Code = w.Currency.Code,
                   RateToEgp = w.Currency.RateToEgp
               }
           }).ToListAsync(ct);

        var bookingListDto = bookingList
            .Select(b => new AdminBookingListDto(
                b.Id,
                b.WorkerFullName,
                b.WorkerFullName,
               workerProfiles.FirstOrDefault(p => p.UserId == b.WorkerId)?.Id??0,
                b.ServiceType,
                b.BookingType,
                b.Quantity,
                workerProfiles.FirstOrDefault(p => p.UserId == b.WorkerId)?.Currency.Code ?? "EGP",
                b.StartDate,
                b.MonthlySalary,
                b.DailySalary,
                b.HourlySalary,
                b.TotalAmount,
                (b.TotalAmount * workerProfiles.FirstOrDefault(p => p.UserId == b.WorkerId)?.Currency.RateToEgp ?? 1),
                b.CommissionAmount,
                b.Status,
                b.IsPaid,
                b.ReplacementCount,                
                b.CreatedAt))
            .ToList();
        return bookingListDto;
    }
}