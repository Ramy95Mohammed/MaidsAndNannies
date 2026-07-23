using MediatR;

namespace MaidsAndNannies.Application.Features.Admin.Commands.RejectPayment;

public sealed record RejectPaymentCommand(int PaymentProofId, string? Reason) : IRequest<Unit>;