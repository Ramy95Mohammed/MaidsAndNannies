using MaidsAndNannies.Application.Features.Worker.Common;
using MediatR;

namespace MaidsAndNannies.Application.Features.Worker.Queries.GetMyProfile;

public sealed record GetMyWorkerProfileQuery(string UserId) : IRequest<WorkerProfileDto>;
