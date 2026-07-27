using MediatR;

namespace MaidsAndNannies.Application.Features.Admin.Commands.ApproveWorker;

public sealed record ApproveWorkerCommand(int WorkerProfileId, string AdminUserId) : IRequest<Unit>;
