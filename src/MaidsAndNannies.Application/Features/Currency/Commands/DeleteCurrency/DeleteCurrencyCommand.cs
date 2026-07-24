using MediatR;

namespace MaidsAndNannies.Application.Features.Currency.Commands.DeleteCurrency;

public sealed record DeleteCurrencyCommand(int Id) : IRequest<Unit>;