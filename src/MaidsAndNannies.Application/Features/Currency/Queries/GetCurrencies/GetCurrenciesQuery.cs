using MediatR;
using MaidsAndNannies.Application.Features.Currency.Common;

namespace MaidsAndNannies.Application.Features.Currency.Queries.GetCurrencies;

public sealed record GetCurrenciesQuery(bool IncludeInactive = false)
    : IRequest<IReadOnlyList<CurrencyDto>>;