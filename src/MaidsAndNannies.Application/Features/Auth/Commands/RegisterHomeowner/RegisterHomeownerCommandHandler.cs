using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Entities.Identity;
using MaidsAndNannies.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ValidationException = MaidsAndNannies.Application.Common.Exceptions.ValidationException;

namespace MaidsAndNannies.Application.Features.Auth.Commands.RegisterHomeowner;

public sealed class RegisterHomeownerCommandHandler(
    UserManager<ApplicationUser> userManager,
    IApplicationDbContext dbContext) : IRequestHandler<RegisterHomeownerCommand, Unit>
{
    public async Task<Unit> Handle(RegisterHomeownerCommand request, CancellationToken cancellationToken)
    {
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

        dbContext.HomeownerProfiles.Add(new HomeownerProfile
        {
            UserId = user.Id,
            City = request.City ?? string.Empty,
            Address = request.Address ?? string.Empty
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
