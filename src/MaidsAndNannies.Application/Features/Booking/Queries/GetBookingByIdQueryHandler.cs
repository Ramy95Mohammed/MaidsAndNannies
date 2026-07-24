using MaidsAndNannies.Application.Common.Helpers;
using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Bookings.Common;
using MaidsAndNannies.Application.Features.Worker.Queries.GetMyProfile;
using MaidsAndNannies.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Bookings.Queries.GetBookingById;

public sealed class GetBookingByIdQueryHandler(
    IApplicationDbContext dbContext,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<GetBookingByIdQuery, BookingDetailDto>
{
    public async Task<BookingDetailDto> Handle(GetBookingByIdQuery request, CancellationToken ct)
    {
        var booking = await dbContext.Bookings
            .Include(b => b.Homeowner)            
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, ct)
            ?? throw new KeyNotFoundException("الحجز غير موجود");

        if (booking.HomeownerId != request.UserId && booking.WorkerId != request.UserId && request.Role != "Admin")
            throw new UnauthorizedAccessException("غير مصرح لك بمشاهدة هذا الحجز");

        bool canRevealDetails = booking.IsPaid || request.Role == "Admin";

        var worker = dbContext.WorkerProfiles
            .Include(u=>u.User)
            .Include(c=>c.Currency)
            .Include(d=>d.Documents)
            .FirstOrDefault(w => w.UserId == booking.WorkerId);
        if (worker != null)
        {
            worker.IsAvailable = false;
        }

        return new BookingDetailDto(
            booking.Id,
            booking.HomeownerId,
            booking.Homeowner.FullName,
            booking.Homeowner.PhoneNumber,
            booking.WorkerId,
            worker.User.FullName,
            canRevealDetails ? worker.User.PhoneNumber : null,
            canRevealDetails ? worker.WhatsAppNumber : null,
            canRevealDetails ? AbsoluteUrlHelper.ToAbsoluteUrl(worker.Documents.FirstOrDefault(d=>d.Type == Domain.Enums.DocumentType.Selfie).DocumentImageUrl, httpContextAccessor) : null,
            null,
            booking.ServiceType,
            booking.BookingType,
            booking.Quantity,
             worker.Currency.Code,
            booking.StartDate,
            booking.EndDate,
            booking.MonthlySalary,
            booking.DailySalary,
            booking.HourlySalary,
            booking.TotalAmount,
            (booking.TotalAmount * worker.Currency.RateToEgp),
            booking.CommissionAmount,
            booking.CommissionType,
            booking.Status,
            booking.IsPaid,
            booking.ReplacementCount,
            booking.AdminNotes,
            booking.CreatedAt);
    }
}