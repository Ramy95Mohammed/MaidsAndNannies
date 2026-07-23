using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Admin.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Admin.Queries.GetPendingPayments;

public sealed class GetPendingPaymentsQueryHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<GetPendingPaymentsQuery, IReadOnlyList<PendingPaymentDto>>
{
    public async Task<IReadOnlyList<PendingPaymentDto>> Handle(GetPendingPaymentsQuery request, CancellationToken ct)
    {
        return await dbContext.PaymentProofs
            .Include(p => p.Homeowner)
            .Where(p => !p.IsConfirmed)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PendingPaymentDto(
                p.Id, p.BookingId, p.Homeowner.FullName, p.Amount,
                p.CommissionAmount,
                p.PaymentMethod, p.TransactionReference, p.ProofImageUrl,
                p.IsConfirmed, p.CreatedAt))
            .ToListAsync(ct);
    }
}