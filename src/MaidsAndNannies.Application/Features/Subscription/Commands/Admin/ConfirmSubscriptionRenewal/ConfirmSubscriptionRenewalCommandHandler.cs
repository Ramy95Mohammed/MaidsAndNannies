using MaidsAndNannies.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Subscription.Commands.Admin.ConfirmSubscriptionRenewal;

public sealed class ConfirmSubscriptionRenewalCommandHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<ConfirmSubscriptionRenewalCommand, Unit>
{
    public async Task<Unit> Handle(ConfirmSubscriptionRenewalCommand request, CancellationToken ct)
    {
        var sub = await dbContext.Subscriptions.FindAsync([request.SubscriptionId], ct)
            ?? throw new KeyNotFoundException("الاشتراك غير موجود");

        sub.IsActive = true;
        sub.PaymentConfirmedBy = request.AdminId;
        sub.PaymentConfirmedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}