using MaidsAndNannies.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Admin.Commands.RejectPayment;

public sealed class RejectPaymentCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<RejectPaymentCommand, Unit>
{
    public async Task<Unit> Handle(RejectPaymentCommand request, CancellationToken ct)
    {
        var paymentProof = await dbContext.PaymentProofs
            .FirstOrDefaultAsync(p => p.Id == request.PaymentProofId, ct)
            ?? throw new KeyNotFoundException("إثبات الدفع غير موجود");

        paymentProof.RejectionReason = request.Reason ?? "تم الرفض من الإدارة";

        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}