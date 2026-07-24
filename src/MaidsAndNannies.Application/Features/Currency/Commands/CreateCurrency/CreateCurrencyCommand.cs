using MediatR;

namespace MaidsAndNannies.Application.Features.Currency.Commands.CreateCurrency;

public sealed record CreateCurrencyCommand(
    string Code,
    string Symbol,
    string NameAr,
    string NameEn,
    decimal RateToEgp,
    bool IsActive) : IRequest<int>;