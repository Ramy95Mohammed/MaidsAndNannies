using MaidsAndNannies.Application.Common.Helpers;
using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Bookings.Common;
using MaidsAndNannies.Domain.Enums;
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
            .ThenInclude(b => b.HomeownerProfile)
            .Include(b => b.Currency)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, ct)
            ?? throw new KeyNotFoundException("الحجز غير موجود");

        if (booking.HomeownerId != request.UserId && booking.WorkerId != request.UserId && request.Role != "Admin")
            throw new UnauthorizedAccessException("غير مصرح لك بمشاهدة هذا الحجز");

        bool canRevealDetails = request.Role == "Admin" || booking.IsPaid || request.UserId == booking.HomeownerId;

        var worker = dbContext.WorkerProfiles
            .Include(u => u.User)
            .Include(c => c.Currency)
            .Include(d => d.Documents)
            .FirstOrDefault(w => w.UserId == booking.WorkerId);

        var workerSelfieDocument = worker?.Documents.FirstOrDefault(d => d.Type == Domain.Enums.DocumentType.Selfie);

        var settings = await dbContext.AppSettings.ToListAsync(ct);

        // حدود الاستبدال: مخصصة لصاحبة المنزل إن وُجدت، وإلا من الإعدادات
        var homeownerProfile = await dbContext.HomeownerProfiles
            .FirstOrDefaultAsync(h => h.UserId == booking.HomeownerId, ct);

        var maxFaultSetting = settings.FirstOrDefault(s => s.Key == "MaxFaultReplacementCount")?.Value;
        var maxPreferenceSetting = settings.FirstOrDefault(s => s.Key == "MaxPreferenceReplacementCount")?.Value;

        var maxFault = homeownerProfile?.MaxFaultReplacementCount
            ?? (int.TryParse(maxFaultSetting, out var mf) ? mf : 3);
        var maxPreference = homeownerProfile?.MaxPreferenceReplacementCount
            ?? (int.TryParse(maxPreferenceSetting, out var mp) ? mp : 1);
        var maxReplacement = Math.Max(maxFault, maxPreference);

        // وضع تحصيل المبلغ عند الدفع
        var billingMode = settings.FirstOrDefault(s => s.Key == "CommissionBillingMode")?.Value ?? "CommissionOnly";
        var requireProof = (settings.FirstOrDefault(s => s.Key == "RequirePaymentProof")?.Value ?? "true") == "true";

        var rateToEgp = booking.Currency?.RateToEgp ?? worker?.Currency?.RateToEgp ?? 1m;
        var currencyCode = booking.Currency?.Code ?? worker?.Currency?.Code ?? "EGP";

        // مرتب العاملة الأول بالجنيه (الذي تتقاضاه في البداية)
        var workerFirstSalaryInEgp = booking.BookingType switch
        {
            BookingType.Daily => booking.DailySalary * booking.Quantity * rateToEgp,
            BookingType.Hourly => booking.HourlySalary * booking.Quantity * rateToEgp,
            _ => booking.MonthlySalary * rateToEgp
        };

        // المبلغ الإجمالي المطلوب من صاحبة المنزل عند الدفع
        var paymentAmount = billingMode == "CommissionPlusSalary"
            ? booking.CommissionAmount + workerFirstSalaryInEgp
            : booking.CommissionAmount;

        var hasReviewed = await dbContext.Reviews
           .AnyAsync(x => x.BookingId == booking.Id && x.ReviewerId == request.UserId, ct);


        return new BookingDetailDto(
            booking.Id,
            booking.HomeownerId,
            booking.Homeowner.FullName,
            booking.Homeowner.PhoneNumber,
            booking.Homeowner.HomeownerProfile.WhatsAppNumber,
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
            maxPreference,
            paymentAmount,
            requireProof,
            hasReviewed);

    }
}