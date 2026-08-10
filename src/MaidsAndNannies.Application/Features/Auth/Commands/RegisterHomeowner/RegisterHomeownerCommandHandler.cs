using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Entities.Identity;
using MaidsAndNannies.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ValidationException = MaidsAndNannies.Application.Common.Exceptions.ValidationException;

namespace MaidsAndNannies.Application.Features.Auth.Commands.RegisterHomeowner;

public sealed class RegisterHomeownerCommandHandler(
    UserManager<ApplicationUser> userManager,
    IApplicationDbContext dbContext) : IRequestHandler<RegisterHomeownerCommand, Unit>
{
    public async Task<Unit> Handle(RegisterHomeownerCommand request, CancellationToken cancellationToken)
    {
        var phoneDigits = NormalizeDigits(request.PhoneNumber);

        var takenPhones = await dbContext.Users
            .Where(u => u.PhoneNumber != null)
            .Select(u => u.PhoneNumber!)
            .ToListAsync(cancellationToken);
        if (takenPhones.Any(p => NormalizeDigits(p) == phoneDigits))
            throw new InvalidOperationException("رقم الهاتف مسجل مسبقاً، لا يمكن إنشاء حساب جديد بهذا الرقم");

        var takenWhatsApps = await dbContext.WorkerProfiles
            .Where(w => w.WhatsAppNumber != null)
            .Select(w => w.WhatsAppNumber!)
            .ToListAsync(cancellationToken);
        var takenHomeownerWhatsApps = await dbContext.HomeownerProfiles
            .Where(h => h.WhatsAppNumber != null && h.WhatsAppNumber != string.Empty)
            .Select(h => h.WhatsAppNumber!)
            .ToListAsync(cancellationToken);
        if (takenWhatsApps.Concat(takenHomeownerWhatsApps).Any(w => NormalizeDigits(w) == phoneDigits))
            throw new InvalidOperationException("رقم الواتساب مسجل مسبقاً على حساب آخر");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            PreferredLanguage = "ar",
            Role = UserRole.Homeowner
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors
                .Select(e => new FluentValidation.Results.ValidationFailure("Identity", e.Description)));
        }

        await userManager.AddToRoleAsync(user, UserRole.Homeowner.ToString());

        try
        {
            
            dbContext.HomeownerProfiles.Add(new HomeownerProfile
            {
                UserId = user.Id,
                City = request.City ?? string.Empty,
                Address = request.Address ?? string.Empty,
                NationalIdNumber = ""
            });

            await dbContext.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
        catch (Exception ex)
        {
            await userManager.DeleteAsync(user);
            throw;
        }

    }

    private static string NormalizeDigits(string? value)
    {
        var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.StartsWith("20") && digits.Length == 12) digits = digits[2..];
        return digits;
    }
}
