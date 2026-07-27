using MaidsAndNannies.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Currency.Commands.UpdateCurrency;

public sealed class UpdateCurrencyCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<UpdateCurrencyCommand, Unit>
{
    public async Task<Unit> Handle(UpdateCurrencyCommand request, CancellationToken ct)
    {
        var currency = await dbContext.Currencies
            .FirstOrDefaultAsync(c => c.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("العملة غير موجودة");

        currency.Code = request.Code;
        currency.Symbol = request.Symbol;
        currency.NameAr = request.NameAr;
        currency.NameEn = request.NameEn;
        currency.RateToEgp = request.RateToEgp;
        currency.IsActive = request.IsActive;
        currency.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}