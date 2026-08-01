using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace MaidsAndNannies.Application.Features.Auth.Commands.ChangePassword;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest<Unit>;

public sealed class ChangePasswordCommandHandler(
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUser) : IRequestHandler<ChangePasswordCommand, Unit>
{
    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        if (request.CurrentPassword == request.NewPassword)
            throw new InvalidOperationException("كلمة المرور الجديدة يجب أن تكون مختلفة عن كلمة المرور الحالية");

        var user = await userManager.FindByIdAsync(currentUser.UserId)
            ?? throw new UnauthorizedAccessException("المستخدم غير موجود");

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("، ", result.Errors.Select(e => e.Description)));

        return Unit.Value;
    }
}