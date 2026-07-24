using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Bookings.Common;
using MaidsAndNannies.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MaidsAndNannies.Application.Features.Bookings.Queries.GetMyBookings;

public sealed class GetMyBookingsQueryHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<GetMyBookingsQuery, IReadOnlyList<BookingListDto>>
{
    public async Task<IReadOnlyList<BookingListDto>> Handle(GetMyBookingsQuery request, CancellationToken ct)
    {

        var bookingList = await dbContext.Bookings
            .Include(b => b.Worker)
            .Where(b => b.HomeownerId == request.UserId)
            .OrderByDescending(b => b.CreatedAt).Select(b => new
            {
                b.Id,
                b.Worker.FullName,
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
            }).ToListAsync(ct);

        var workerProfiles = await dbContext.WorkerProfiles.Where(p => bookingList.Select(b => b.WorkerId).Contains(p.UserId))
            .Select(w=> new WorkerProfile
            {    
                Id = w.Id,
                UserId = w.UserId,
                Currency = new Domain.Entities.Currency {
                    Code = w.Currency.Code,
                    RateToEgp = w.Currency.RateToEgp
                }
            }).ToListAsync(ct);

        var bookingListDto = bookingList.Select(b => new BookingListDto(
                b.Id,
                b.FullName,
                workerProfiles.FirstOrDefault(p => p.UserId == b.WorkerId)?.Id ?? 0,
                b.ServiceType,
                b.BookingType,
                b.Quantity,
                workerProfiles.FirstOrDefault(p=>p.UserId == b.WorkerId)?.Currency.Code??"EGP",
                b.StartDate,
                b.MonthlySalary,
                b.DailySalary,
                b.HourlySalary,
                b.TotalAmount,
               (b.TotalAmount * workerProfiles.FirstOrDefault(p => p.UserId == b.WorkerId)?.Currency.RateToEgp ?? 1) ,
                b.CommissionAmount,
                b.Status,
                b.IsPaid,
                b.ReplacementCount,
                b.CreatedAt)).ToList();            
                
            return bookingListDto;
    }
}