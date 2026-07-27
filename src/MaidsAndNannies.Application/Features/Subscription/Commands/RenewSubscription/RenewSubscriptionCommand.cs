using MaidsPlatform.API.Domain.Enums;
using MediatR;

namespace MaidsAndNannies.Application.Features.Subscription.Commands.RenewSubscription;

public sealed record RenewSubscriptionCommand(
    int SubscriptionId,
    string HomeownerId,
    PaymentMethod PaymentMethod,
    decimal Amount,
    string? TransactionReference,
    Stream? ProofImageContent,
    string? ProofImageFileName) : IRequest<Unit>;