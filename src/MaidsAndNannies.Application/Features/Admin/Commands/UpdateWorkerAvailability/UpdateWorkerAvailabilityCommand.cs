using MediatR;

namespace MaidsAndNannies.Application.Features.Admin.Commands.UpdateWorkerAvailability;

public sealed record UpdateWorkerAvailabilityCommand(int WorkerId, bool IsAvailable) : IRequest<Unit>;