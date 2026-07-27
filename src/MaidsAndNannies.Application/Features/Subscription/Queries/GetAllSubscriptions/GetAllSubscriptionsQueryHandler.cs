using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Subscription.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Subscription.Queries.GetAllSubscriptions;

public sealed class GetAllSubscriptionsQueryHandler(
    IApplicationDbContext dbContext)
    : IRequestHandler<GetAllSubscriptionsQuery, IReadOnlyList<SubscriptionDto>>
{
    public async Task<IReadOnlyList<SubscriptionDto>> Handle(GetAllSubscriptionsQuery request, CancellationToken ct)
    {
        return await dbContext.Subscriptions
            .Include(s => s.Homeowner)
            .OrderByDescending(s => s.IsActive)
            .ThenBy(s => s.EndDate)
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