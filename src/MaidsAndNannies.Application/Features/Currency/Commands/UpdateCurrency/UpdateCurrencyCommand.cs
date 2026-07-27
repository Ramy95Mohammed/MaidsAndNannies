using MediatR;

namespace MaidsAndNannies.Application.Features.Currency.Commands.UpdateCurrency;

public sealed record UpdateCurrencyCommand(
    int Id,
    string Code,
    string Symbol,
    string NameAr,
    string NameEn,
    decimal RateToEgp,
    bool IsActive) : IRequest<Unit>;