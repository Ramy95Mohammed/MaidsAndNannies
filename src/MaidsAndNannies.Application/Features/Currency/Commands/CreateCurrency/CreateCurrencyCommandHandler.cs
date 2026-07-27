using MaidsAndNannies.Application.Common.Interfaces;
using MediatR;

namespace MaidsAndNannies.Application.Features.Currency.Commands.CreateCurrency;

public sealed class CreateCurrencyCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<CreateCurrencyCommand, int>
{
    public async Task<int> Handle(CreateCurrencyCommand request, CancellationToken ct)
    {
        var currency = new Domain.Entities.Currency
        {
            Code = request.Code,
            Symbol = request.Symbol,
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            RateToEgp = request.RateToEgp,
            IsActive = request.IsActive
        };

        dbContext.Currencies.Add(currency);
        await dbContext.SaveChangesAsync(ct);
        return currency.Id;
    }
}