using MaidsAndNannies.Application.Features.Admin.Common;
using MediatR;

namespace MaidsAndNannies.Application.Features.Admin.Queries.GetPendingHomeowners;

public sealed record GetPendingHomeownersQuery : IRequest<IReadOnlyList<PendingHomeownerDto>>;
