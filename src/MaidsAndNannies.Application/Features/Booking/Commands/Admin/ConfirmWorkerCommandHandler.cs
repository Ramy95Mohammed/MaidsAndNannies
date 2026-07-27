using MaidsAndNannies.Application.Common.Interfaces;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.Admin;

public sealed class ConfirmWorkerCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<ConfirmWorkerCommand, Unit>
{
    public async Task<Unit> Handle(ConfirmWorkerCommand request, CancellationToken ct)
    {
        var booking = await dbContext.Bookings.FindAsync([request.BookingId], ct)
            ?? throw new KeyNotFoundException("الحجز غير موجود");

        if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.ReplacementRequested)
            throw new InvalidOperationException("لا يمكن تأكيد العاملة في هذه الحالة");

        if (booking.Status == BookingStatus.ReplacementRequested)
        {
            // الحجز كان مدفوعاً مسبقاً
            if (booking.OutstandingAmount > 0)
            {
                // يجب دفع الفرق أولاً
                booking.CommissionAmount += booking.OutstandingAmount;
                booking.Status = BookingStatus.WaitingPayment;
                booking.OutstandingAmount = 0;
            }
            else
            {
                // لا يوجد فرق — نرجع الحالة إلى مدفوع مباشرة
                booking.Status = BookingStatus.Paid;
            }
        }
        else
        {
            // حجز جديد غير مدفوع
            booking.Status = BookingStatus.WorkerConfirmed;
        }

        // تعيين العاملة كغير متاحة
        var worker = await dbContext.WorkerProfiles
            .FirstOrDefaultAsync(w => w.UserId == booking.WorkerId, ct);
        if (worker is not null)
            worker.IsAvailable = false;

        booking.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}