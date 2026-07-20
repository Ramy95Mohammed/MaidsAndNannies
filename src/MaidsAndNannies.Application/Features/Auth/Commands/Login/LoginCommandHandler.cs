using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Auth.Common;
using MaidsAndNannies.Domain.Entities.Identity;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace MaidsAndNannies.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<LoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.Single();

        var (accessToken, expiresAtUtc) = jwtTokenGenerator.GenerateToken(user, role);
        return new AuthResponseDto(accessToken, expiresAtUtc, user.FullName, role,
            user.PreferredLanguage, VerificationStatus.Pending);
    }
}
