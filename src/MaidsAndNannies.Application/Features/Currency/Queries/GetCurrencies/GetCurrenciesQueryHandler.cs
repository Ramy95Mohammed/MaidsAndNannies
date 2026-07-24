using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Currency.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Currency.Queries.GetCurrencies;

public sealed class GetCurrenciesQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetCurrenciesQuery, IReadOnlyList<CurrencyDto>>
{
    public async Task<IReadOnlyList<CurrencyDto>> Handle(GetCurrenciesQuery request, CancellationToken ct)
    {
        var query = dbContext.Currencies.AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.Code)
            .Select(c => new CurrencyDto(
                c.Id, c.Code, c.Symbol, c.NameAr, c.NameEn, c.RateToEgp, c.IsActive))
            .ToListAsync(ct);
    }
}