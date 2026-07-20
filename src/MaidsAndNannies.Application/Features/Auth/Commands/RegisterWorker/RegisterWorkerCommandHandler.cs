using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Contracts;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Entities.Identity;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ValidationException = MaidsAndNannies.Application.Common.Exceptions.ValidationException;

namespace MaidsAndNannies.Application.Features.Auth.Commands.RegisterWorker;

public sealed class RegisterWorkerCommandHandler(
    UserManager<ApplicationUser> userManager,
    IApplicationDbContext dbContext,
    IFileStorage fileStorage) : IRequestHandler<RegisterWorkerCommand, Unit>
{
    public async Task<Unit> Handle(RegisterWorkerCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            PreferredLanguage = "ar",
            Role = UserRole.Worker
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors
                .Select(e => new FluentValidation.Results.ValidationFailure("Identity", e.Description)));
        }

        await userManager.AddToRoleAsync(user, UserRole.Worker.ToString());

        var workerProfile = new WorkerProfile
        {
            UserId = user.Id,
            NationalityId = request.NationalityId,
            CountryId = request.CountryId,
            StateId = request.StateId,
            Bio = request.Bio ?? string.Empty,
            ExperienceYears = request.ExperienceYears,
            MonthlyRate = request.MonthlyRate,
            VerificationStatus = VerificationStatus.Pending
        };

        if (request.SelfieImageContent is not null && request.SelfieImageFileName is not null)
        {
            var url = await fileStorage.SavePublicAsync(
                request.SelfieImageContent, request.SelfieImageFileName, $"workers/{user.Id}", cancellationToken);

            workerProfile.Documents = new List<WorkerDocument>
            {
                new() { Type = DocumentType.Selfie, DocumentImageUrl = url }
            };
        }

        dbContext.WorkerProfiles.Add(workerProfile);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
