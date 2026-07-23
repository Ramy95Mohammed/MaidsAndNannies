using MediatR;

namespace MaidsAndNannies.Application.Features.Subscription.Commands.Admin.ConfirmSubscriptionRenewal;

public sealed record ConfirmSubscriptionRenewalCommand(int SubscriptionId, string AdminId) : IRequest<Unit>;