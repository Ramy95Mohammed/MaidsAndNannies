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
            if (booking.OutstandingAmount > 0)
            {
                // أكبر — يحتاج دفع الفرق (CommissionAmount محدث مسبقاً)
                booking.Status = BookingStatus.WaitingPayment;
                //booking.OutstandingAmount = 0;
            }
            else
            {
                // أقل أو نفس المبلغ — مدفوع مسبقاً
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