using MediatR;
using MaidsAndNannies.Application.Features.Subscription.Common;

namespace MaidsAndNannies.Application.Features.Subscription.Queries.GetMySubscriptions;

public sealed record GetMySubscriptionsQuery(string HomeownerId) : IRequest<IReadOnlyList<SubscriptionDto>>;