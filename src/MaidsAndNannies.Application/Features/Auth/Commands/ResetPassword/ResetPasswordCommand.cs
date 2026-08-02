using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Auth.Commands.ResetPassword;

public sealed record ResetPasswordCommand(string Email, string Code, string NewPassword) : IRequest<Unit>;

public sealed class ResetPasswordCommandHandler(
    UserManager<ApplicationUser> userManager,
    IApplicationDbContext dbContext) : IRequestHandler<ResetPasswordCommand, Unit>
{
    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(request.Email)
            ?? throw new InvalidOperationException("البريد الإلكتروني غير مسجل");

        var now = DateTime.UtcNow;

        var requestEntity = await dbContext.PasswordResetRequests
            .Where(r => r.UserId == user.Id && r.Email == request.Email
                && (r.Status == PasswordResetStatus.Pending || r.Status == PasswordResetStatus.Sent))
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (requestEntity is null)
            throw new InvalidOperationException("لا يوجد طلب استعادة كلمة مرور لهذا البريد");

        if (requestEntity.ExpiresAt < now)
        {
            requestEntity.Status = PasswordResetStatus.Expired;
            requestEntity.ResolvedAt = now;
            await dbContext.SaveChangesAsync(ct);
            throw new InvalidOperationException("انتهت صلاحية الكود، يرجى طلب كود جديد من صفحة تسجيل الدخول");
        }

        if (requestEntity.Code != request.Code)
            throw new InvalidOperationException("الكود غير صحيح");

        if (request.NewPassword.Length < 8)
            throw new InvalidOperationException("كلمة المرور الجديدة يجب أن تكون 8 أحرف على الأقل");

        var removeResult = await userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
            throw new InvalidOperationException("حدث خطأ أثناء تعيين كلمة المرور");

        var addResult = await userManager.AddPasswordAsync(user, request.NewPassword);
        if (!addResult.Succeeded)
            throw new InvalidOperationException(string.Join("، ", addResult.Errors.Select(e => e.Description)));

        requestEntity.Status = PasswordResetStatus.Used;
        requestEntity.ResolvedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(ct);

        return Unit.Value;
    }
}