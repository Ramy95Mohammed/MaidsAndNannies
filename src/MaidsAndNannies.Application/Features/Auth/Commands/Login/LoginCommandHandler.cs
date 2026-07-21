using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Auth.Common;
using MaidsAndNannies.Domain.Entities.Identity;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    UserManager<ApplicationUser> userManager,
    IApplicationDbContext dbContext,
    IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<LoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.Single();

        var (verificationStatus, verificationNotes) = await GetVerificationAsync(user.Id, role, cancellationToken);

        var (accessToken, expiresAtUtc) = jwtTokenGenerator.GenerateToken(user, role);
        return new AuthResponseDto(accessToken, expiresAtUtc, user.FullName, role,
            user.PreferredLanguage, verificationStatus, verificationNotes);
    }

    /// <summary>
    /// Admins/Verifiers have no profile row and are always "approved". Workers
    /// and Homeowners are only usable once an Admin approves their profile, so
    /// login always succeeds but the frontend uses this to gate the app (show a
    /// "your account is under review" screen for Pending/Rejected).
    /// </summary>
    private async Task<(VerificationStatus Status, string? Notes)> GetVerificationAsync(
        string userId, string role, CancellationToken cancellationToken)
    {
        if (role == UserRole.Worker.ToString())
        {
            var worker = await dbContext.WorkerProfiles
                .Where(w => w.UserId == userId)
                .Select(w => new { w.VerificationStatus, w.VerificationNotes })
                .FirstOrDefaultAsync(cancellationToken);

            return worker is null
                ? (VerificationStatus.Pending, null)
                : (worker.VerificationStatus, worker.VerificationNotes);
        }

        if (role == UserRole.Homeowner.ToString())
        {
            var homeowner = await dbContext.HomeownerProfiles
                .Where(h => h.UserId == userId)
                .Select(h => new { h.VerificationStatus, h.VerificationNotes })
                .FirstOrDefaultAsync(cancellationToken);

            return homeowner is null
                ? (VerificationStatus.Pending, null)
                : (homeowner.VerificationStatus, homeowner.VerificationNotes);
        }

        return (VerificationStatus.Approved, null);
    }
}
