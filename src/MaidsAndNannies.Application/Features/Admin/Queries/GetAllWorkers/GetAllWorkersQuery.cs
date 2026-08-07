using MaidsAndNannies.Application.Features.Admin.Common;
using MediatR;

namespace MaidsAndNannies.Application.Features.Admin.Queries.GetAllWorkers;

public sealed record GetAllWorkersQuery() : IRequest<IReadOnlyList<AdminWorkerDto>>;