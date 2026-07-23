using MediatR;

namespace MaidsAndNannies.Application.Features.Admin.Commands.ConfirmPayment;

public sealed record ConfirmPaymentCommand(int PaymentProofId, string AdminId) : IRequest<Unit>;