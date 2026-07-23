using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Subscription.Commands.RenewSubscription;

public sealed class RenewSubscriptionCommandHandler(
    IApplicationDbContext dbContext,
    IFileStorage fileStorage)
    : IRequestHandler<RenewSubscriptionCommand, Unit>
{
    public async Task<Unit> Handle(RenewSubscriptionCommand request, CancellationToken ct)
    {
        var sub = await dbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId && s.HomeownerId == request.HomeownerId, ct)
            ?? throw new KeyNotFoundException("الاشتراك غير موجود");

        string? proofUrl = null;
        if (request.ProofImageContent is not null && request.ProofImageFileName is not null)
            proofUrl = await fileStorage.SavePublicAsync(
                request.ProofImageContent, request.ProofImageFileName,
                $"subscriptions/{request.SubscriptionId}", ct);

        var newSub = new Domain.Entities.Subscription
        {
            HomeownerId = request.HomeownerId,
            PlanType = sub.PlanType,
            Amount = request.Amount,
            StartDate = sub.EndDate,
            EndDate = sub.EndDate.AddDays(30),
            IsActive = false,
            PaymentMethod = request.PaymentMethod,
            PaymentProofImageUrl = proofUrl,
            TransactionReference = request.TransactionReference
        };

        dbContext.Subscriptions.Add(newSub);
        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}