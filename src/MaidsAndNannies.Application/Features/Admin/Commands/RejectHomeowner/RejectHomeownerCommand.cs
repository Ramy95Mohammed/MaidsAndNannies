using MediatR;

namespace MaidsAndNannies.Application.Features.Admin.Commands.RejectHomeowner;

public sealed record RejectHomeownerCommand(int HomeownerProfileId, string AdminUserId, string Reason) : IRequest<Unit>;
