using MediatR;
using MaidsAndNannies.Application.Features.Admin.Common;

namespace MaidsAndNannies.Application.Features.Admin.Queries.GetPendingPayments;

public sealed record GetPendingPaymentsQuery : IRequest<IReadOnlyList<PendingPaymentDto>>;