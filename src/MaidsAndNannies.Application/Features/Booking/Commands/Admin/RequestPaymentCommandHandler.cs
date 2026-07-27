using MaidsAndNannies.Application.Common.Interfaces;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.Admin;

public sealed class RequestPaymentCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<RequestPaymentCommand, Unit>
{
    public async Task<Unit> Handle(RequestPaymentCommand request, CancellationToken ct)
    {
        var booking = await dbContext.Bookings.FindAsync([request.BookingId], ct)
            ?? throw new KeyNotFoundException("الحجز غير موجود");

        if (booking.Status != BookingStatus.WorkerConfirmed)
            throw new InvalidOperationException("يجب تأكيد العاملة أولاً");

        booking.Status = BookingStatus.WaitingPayment;
        booking.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}