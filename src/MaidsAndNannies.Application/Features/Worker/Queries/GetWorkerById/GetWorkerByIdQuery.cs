using MaidsAndNannies.Application.Features.Worker.Common;
using MediatR;

namespace MaidsAndNannies.Application.Features.Worker.Queries.GetWorkerById;

public sealed record GetWorkerByIdQuery(int WorkerProfileId) : IRequest<WorkerDetailDto>;
