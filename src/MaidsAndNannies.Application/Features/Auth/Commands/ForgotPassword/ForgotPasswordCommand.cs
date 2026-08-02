using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace MaidsAndNannies.Application.Features.Auth.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : IRequest;

public sealed class ForgotPasswordCommandHandler(
    UserManager<ApplicationUser> userManager,
    IApplicationDbContext dbContext) : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null) return;

        var now = DateTime.UtcNow;

        var old = await dbContext.PasswordResetRequests
            .Where(r => r.UserId == user.Id
                && (r.Status == PasswordResetStatus.Pending || r.Status == PasswordResetStatus.Sent))
            .ToListAsync(ct);

        foreach (var r in old)
        {
            r.Status = PasswordResetStatus.Expired;
            r.ResolvedAt = now;
        }

        dbContext.PasswordResetRequests.Add(new PasswordResetRequest
        {
            UserId = user.Id,
            Email = user.Email ?? request.Email,
            Code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString(),
            Status = PasswordResetStatus.Pending,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(30)
        });

        await dbContext.SaveChangesAsync(ct);
    }
}