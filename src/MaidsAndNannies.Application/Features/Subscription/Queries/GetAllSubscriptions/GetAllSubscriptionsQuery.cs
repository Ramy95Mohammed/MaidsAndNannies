using MediatR;
using MaidsAndNannies.Application.Features.Subscription.Common;

namespace MaidsAndNannies.Application.Features.Subscription.Queries.GetAllSubscriptions;

public sealed record GetAllSubscriptionsQuery : IRequest<IReadOnlyList<SubscriptionDto>>;