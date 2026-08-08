using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Bookings.Common;
using MaidsAndNannies.Application.Features.Worker.Common;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MaidsAndNannies.Application.Features.Bookings.Queries.GetMyBookings;

public sealed class GetMyBookingsQueryHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<GetMyBookingsQuery, PagedResult<BookingListDto>>
{
    public async Task<PagedResult<BookingListDto>> Handle(GetMyBookingsQuery request, CancellationToken ct)
    {
        if (request.Page < 1) request = request with { Page = 1 };
        if (request.PageSize < 1 || request.PageSize > 50) request = request with { PageSize = 10 };

        var query = dbContext.Bookings
            .Where(b => request.Role == "Worker" ? b.WorkerId == request.UserId : b.HomeownerId == request.UserId);

        if (request.Status.HasValue)
            query = query.Where(b => b.Status == (BookingStatus)request.Status.Value);
        if (request.BookingType.HasValue)
            query = query.Where(b => b.BookingType == (BookingType)request.BookingType.Value);
        if (request.FromDate.HasValue)
            query = query.Where(b => b.StartDate >= request.FromDate.Value.Date);
        if (request.ToDate.HasValue)
            query = query.Where(b => b.StartDate < request.ToDate.Value.Date.AddDays(1));
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = request.Role == "Worker"
                ? query.Where(b => b.Homeowner.FullName.Contains(search))
                : query.Where(b => b.Worker.FullName.Contains(search));
        }

        var totalCount = await query.CountAsync(ct);

        var bookingList = await query
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new
            {
                b.Id,
                WorkerName = b.Worker.FullName,
                HomeownerName = b.Homeowner.FullName,
                HomeownerId = b.HomeownerId,
                HomeownerPhone = b.Homeowner.PhoneNumber,
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
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
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

        var homeownerProfiles = await dbContext.HomeownerProfiles
            .Where(h => bookingList.Select(b => b.HomeownerId).Contains(h.UserId))
            .Select(h => new { h.UserId, h.WhatsAppNumber })
            .ToListAsync(ct);

        var bookingListDto = bookingList.Select(b => new BookingListDto(
                b.Id,
                b.WorkerName,
                b.HomeownerName,
                b.HomeownerPhone,
                homeownerProfiles.FirstOrDefault(p => p.UserId == b.HomeownerId)?.WhatsAppNumber,                
                workerProfiles.FirstOrDefault(p => p.UserId == b.WorkerId)?.Id ?? 0,
                b.ServiceType,
                b.BookingType,
                b.Quantity,
                workerProfiles.FirstOrDefault(p => p.UserId == b.WorkerId)?.Currency.Code ?? "EGP",
                b.StartDate,
                b.MonthlySalary,
                b.DailySalary,
                b.HourlySalary,
                b.TotalAmount,
                b.TotalAmount * workerProfiles.FirstOrDefault(p => p.UserId == b.WorkerId)?.Currency.RateToEgp ?? 1m,
                b.CommissionAmount,
                b.Status,
                b.IsPaid,
                b.ReplacementCount,
                b.CreatedAt,
                dbContext.Reviews.Any(x => x.BookingId == b.Id && x.ReviewerId == request.UserId)
            )).ToList();

        return new PagedResult<BookingListDto>(bookingListDto, totalCount, request.Page, request.PageSize);
    }
}