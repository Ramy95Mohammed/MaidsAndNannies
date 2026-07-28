using MaidsAndNannies.Application.Common.Helpers;
using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Bookings.Common;
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
            .Include(b => b.Currency)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, ct)
            ?? throw new KeyNotFoundException("الحجز غير موجود");

        if (booking.HomeownerId != request.UserId && booking.WorkerId != request.UserId && request.Role != "Admin")
            throw new UnauthorizedAccessException("غير مصرح لك بمشاهدة هذا الحجز");

        bool canRevealDetails = booking.IsPaid || request.Role == "Admin";

        var worker = dbContext.WorkerProfiles
            .Include(u => u.User)
            .Include(c => c.Currency)
            .Include(d => d.Documents)
            .FirstOrDefault(w => w.UserId == booking.WorkerId);

        var workerSelfieDocument = worker?.Documents.FirstOrDefault(d => d.Type == Domain.Enums.DocumentType.Selfie);

        // ملاحظة: الحد الأقصى بقى منفصل حسب سبب الاستبدال (تقصير/رغبة شخصية).
        // القيمة هنا هي الأعلى بينهما للعرض فقط في الواجهة الحالية — يفضّل لاحقاً تعديل
        // BookingDetailDto ليعرض القيمتين منفصلتين (maxFaultReplacement / maxPreferenceReplacement).
        var replacementSettings = await dbContext.AppSettings
            .Where(s => s.Key == "MaxFaultReplacementCount" || s.Key == "MaxPreferenceReplacementCount")
            .ToListAsync(ct);

        var maxFault = int.TryParse(replacementSettings.FirstOrDefault(s => s.Key == "MaxFaultReplacementCount")?.Value, out var mf) ? mf : 3;
        var maxPreference = int.TryParse(replacementSettings.FirstOrDefault(s => s.Key == "MaxPreferenceReplacementCount")?.Value, out var mp) ? mp : 1;
        var maxReplacement = Math.Max(maxFault, maxPreference);

        var rateToEgp = booking.Currency?.RateToEgp ?? worker?.Currency?.RateToEgp ?? 1m;
        var currencyCode = booking.Currency?.Code ?? worker?.Currency?.Code ?? "EGP";     

        return new BookingDetailDto(
            booking.Id,
            booking.HomeownerId,
            booking.Homeowner.FullName,
            booking.Homeowner.PhoneNumber,
            booking.WorkerId,
            worker?.User.FullName ?? "",
            canRevealDetails ? worker?.User.PhoneNumber : null,
            canRevealDetails ? worker?.WhatsAppNumber : null,
            canRevealDetails ? AbsoluteUrlHelper.ToAbsoluteUrl(workerSelfieDocument?.DocumentImageUrl ?? "", httpContextAccessor) : null,
            worker?.NationalityId,
            booking.ServiceType,
            booking.BookingType,
            booking.Quantity,
            currencyCode,
            booking.StartDate,
            booking.EndDate,
            booking.MonthlySalary,
            booking.DailySalary,
            booking.HourlySalary,
            booking.TotalAmount,
            booking.TotalAmount * rateToEgp,
            booking.CommissionAmount,
            booking.CommissionType,
            booking.Status,
            booking.IsPaid,
            booking.ReplacementCount,
            maxReplacement,
            booking.AdminNotes,
            booking.CreatedAt,
            booking.JobPostId,
             booking.OutstandingAmount,
    maxFault,
    maxPreference);
    }
}