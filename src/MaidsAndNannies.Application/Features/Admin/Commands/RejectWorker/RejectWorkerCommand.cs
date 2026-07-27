using MediatR;

namespace MaidsAndNannies.Application.Features.Admin.Commands.RejectWorker;

public sealed record RejectWorkerCommand(int WorkerProfileId, string AdminUserId, string Reason) : IRequest<Unit>;
