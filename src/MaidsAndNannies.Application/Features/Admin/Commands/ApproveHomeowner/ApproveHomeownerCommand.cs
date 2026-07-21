using MediatR;

namespace MaidsAndNannies.Application.Features.Admin.Commands.ApproveHomeowner;

public sealed record ApproveHomeownerCommand(int HomeownerProfileId, string AdminUserId) : IRequest<Unit>;
