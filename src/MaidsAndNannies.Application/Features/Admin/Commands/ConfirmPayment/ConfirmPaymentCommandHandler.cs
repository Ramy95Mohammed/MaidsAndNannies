using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Admin.Commands.ConfirmPayment;

public sealed class ConfirmPaymentCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<ConfirmPaymentCommand, Unit>
{
    public async Task<Unit> Handle(ConfirmPaymentCommand request, CancellationToken ct)
    {
        try
        {
            var paymentProof = await dbContext.PaymentProofs
                .Include(b=>b.Booking)                
            .FirstOrDefaultAsync(p => p.Id == request.PaymentProofId, ct)
            ?? throw new KeyNotFoundException("إثبات الدفع غير موجود");

            paymentProof.IsConfirmed = true;
            paymentProof.ConfirmedBy = request.AdminId;
            paymentProof.ConfirmedAt = DateTime.UtcNow;

            paymentProof.Booking.Status = BookingStatus.Paid;
            paymentProof.Booking.IsPaid = true;            
            paymentProof.Booking.PaidAt = DateTime.UtcNow;
            paymentProof.Booking.PaymentConfirmedBy = request.AdminId;
            paymentProof.Booking.PaymentConfirmedAt = DateTime.UtcNow;          

            await dbContext.SaveChangesAsync(ct);
            return Unit.Value;
        }
        catch (Exception ex)
        {
            throw;
        }
        
    }
}