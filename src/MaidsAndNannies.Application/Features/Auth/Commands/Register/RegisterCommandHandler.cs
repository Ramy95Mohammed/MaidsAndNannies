using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Auth.Common;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Entities.Identity;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ValidationException = MaidsAndNannies.Application.Common.Exceptions.ValidationException;

namespace MaidsAndNannies.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler(
    UserManager<ApplicationUser> userManager,
    IApplicationDbContext dbContext,
    IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var role = request.AccountType == AccountType.Worker ? UserRole.Worker : UserRole.Homeowner;

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            PreferredLanguage = request.PreferredLanguage,
            Role = role
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors
                .Select(e => new FluentValidation.Results.ValidationFailure("Identity", e.Description)));
        }

        await userManager.AddToRoleAsync(user, role.ToString());

        if (role == UserRole.Worker)
        {
            dbContext.WorkerProfiles.Add(new WorkerProfile
            {
                UserId = user.Id,
                NationalityId = request.NationalityId,
                CityId = request.CurrentCityId
            });
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var (accessToken, expiresAtUtc) = jwtTokenGenerator.GenerateToken(user, role.ToString());
        return new AuthResponseDto(accessToken, expiresAtUtc, user.FullName, role.ToString(),
            user.PreferredLanguage, VerificationStatus.Pending);
    }
}
