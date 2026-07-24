using MaidsAndNannies.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Currency.Commands.DeleteCurrency;

public sealed class DeleteCurrencyCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<DeleteCurrencyCommand, Unit>
{
    public async Task<Unit> Handle(DeleteCurrencyCommand request, CancellationToken ct)
    {
        var currency = await dbContext.Currencies
            .FirstOrDefaultAsync(c => c.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("العملة غير موجودة");

        dbContext.Currencies.Remove(currency);
        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}