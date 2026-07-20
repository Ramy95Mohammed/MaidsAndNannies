using MaidsAndNannies.Application.Features.Auth.Common;
using MediatR;

namespace MaidsAndNannies.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;
