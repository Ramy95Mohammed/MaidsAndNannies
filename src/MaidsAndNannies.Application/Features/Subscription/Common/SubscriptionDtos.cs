using MaidsPlatform.API.Domain.Enums;

namespace MaidsAndNannies.Application.Features.Subscription.Common;

public sealed record SubscriptionDto(
    int Id,
    string HomeownerName,
    decimal Amount,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    int DaysRemaining,
    DateTime CreatedAt);

public sealed record RenewSubscriptionRequest(
    PaymentMethod PaymentMethod,
    decimal Amount,
    string? TransactionReference);