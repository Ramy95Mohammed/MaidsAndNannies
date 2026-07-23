using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Subscription.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Subscription.Queries.GetMySubscriptions;

public sealed class GetMySubscriptionsQueryHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<GetMySubscriptionsQuery, IReadOnlyList<SubscriptionDto>>
{
    public async Task<IReadOnlyList<SubscriptionDto>> Handle(GetMySubscriptionsQuery request, CancellationToken ct)
    {
        return await dbContext.Subscriptions
            .Include(s => s.Homeowner)
            .Where(s => s.HomeownerId == request.HomeownerId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SubscriptionDto(
                s.Id,
                s.Homeowner.FullName,
                s.Amount,                
                s.StartDate,
                s.EndDate,
                s.IsActive,
                (s.EndDate - DateTime.UtcNow).Days + 1,
                s.CreatedAt))
            .ToListAsync(ct);
    }
}