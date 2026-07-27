using MaidsAndNannies.Application.Common.Interfaces;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.Admin;

public sealed class ConfirmPaymentCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<ConfirmPaymentCommand, Unit>
{
    public async Task<Unit> Handle(ConfirmPaymentCommand request, CancellationToken ct)
    {
        var booking = await dbContext.Bookings.FindAsync([request.BookingId], ct)
            ?? throw new KeyNotFoundException("الحجز غير موجود");

        if (booking.Status != BookingStatus.WaitingPayment && booking.Status != BookingStatus.PaymentSubmitted)
            throw new InvalidOperationException("الحجز ليس بانتظار الدفع");

        booking.Status = BookingStatus.Paid;
        booking.IsPaid = true;
        booking.PaidAt = DateTime.UtcNow;
        booking.PaymentConfirmedBy = request.AdminId;
        booking.PaymentConfirmedAt = DateTime.UtcNow;
        booking.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}