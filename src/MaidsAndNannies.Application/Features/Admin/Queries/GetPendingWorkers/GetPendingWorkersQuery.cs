using MaidsAndNannies.Application.Features.Admin.Common;
using MediatR;

namespace MaidsAndNannies.Application.Features.Admin.Queries.GetPendingWorkers;

public sealed record GetPendingWorkersQuery : IRequest<IReadOnlyList<PendingWorkerDto>>;
