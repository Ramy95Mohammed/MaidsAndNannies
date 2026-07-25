using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Admin.Commands.RegisterHomeowner;

public sealed class AdminRegisterHomeownerCommandHandler(
    IApplicationDbContext dbContext,
    UserManager<Domain.Entities.Identity.ApplicationUser> userManager)
    : IRequestHandler<AdminRegisterHomeownerCommand, string>
{
    public async Task<string> Handle(AdminRegisterHomeownerCommand request, CancellationToken ct)
    {
        var user = new Domain.Entities.Identity.ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FullName = request.FullName,
            Role = UserRole.Homeowner,
            PreferredLanguage = "ar"
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, "Homeowner");

        dbContext.HomeownerProfiles.Add(new HomeownerProfile
        {
            UserId = user.Id,
            NationalIdNumber = request.NationalIdNumber,
            NationalIdImage = "(admin-created)",
            SelfieImage = "(admin-created)",
            Address = request.Address,
            City = request.City,
            VerificationStatus = VerificationStatus.Approved,
            VerifiedBy = request.AdminUserId,
            VerifiedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(ct);
        return user.Id;
    }
}