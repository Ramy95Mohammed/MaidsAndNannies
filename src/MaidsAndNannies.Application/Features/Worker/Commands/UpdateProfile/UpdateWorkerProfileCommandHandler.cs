using MaidsAndNannies.Application.Common.Exceptions;
using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Contracts;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Worker.Commands.UpdateProfile;

public sealed class UpdateWorkerProfileCommandHandler(IApplicationDbContext dbContext, IFileStorage fileStorage)
    : IRequestHandler<UpdateWorkerProfileCommand, Unit>
{
    public async Task<Unit> Handle(UpdateWorkerProfileCommand request, CancellationToken cancellationToken)
    {
        var workerProfile = await dbContext.WorkerProfiles
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (workerProfile is null)
            throw new NotFoundException("WorkerProfile", request.UserId);

        workerProfile.NationalityId = request.NationalityId ?? workerProfile.NationalityId;
        workerProfile.NationalIdNumber = request.NationalIdNumber ?? workerProfile.NationalIdNumber;
        workerProfile.WhatsAppNumber = request.WhatsAppNumber ?? workerProfile.WhatsAppNumber;
        workerProfile.PassportNumber = request.PassportNumber ?? workerProfile.PassportNumber;
        workerProfile.PassportExpiryDate = request.PassportExpiryDate ?? workerProfile.PassportExpiryDate;
        workerProfile.PassportCountry = request.PassportCountry ?? workerProfile.PassportCountry;

        workerProfile.CountryId = request.CountryId ?? workerProfile.CountryId;
        workerProfile.StateId = request.StateId ?? workerProfile.StateId;
        workerProfile.CityId = request.CityId ?? workerProfile.CityId;
        workerProfile.Address = request.Address ?? workerProfile.Address;

        workerProfile.Bio = request.Bio ?? workerProfile.Bio;
        workerProfile.ExperienceYears = request.ExperienceYears ?? workerProfile.ExperienceYears;
        workerProfile.Languages = request.Languages ?? workerProfile.Languages;
        workerProfile.PreviousEmployer = request.PreviousEmployer ?? workerProfile.PreviousEmployer;
        workerProfile.IsLiveIn = request.IsLiveIn ?? workerProfile.IsLiveIn;
        workerProfile.IsAvailable = request.IsAvailable ?? workerProfile.IsAvailable;

        workerProfile.HourlyRate = request.HourlyRate ?? workerProfile.HourlyRate;
        workerProfile.MonthlyRate = request.MonthlyRate ?? workerProfile.MonthlyRate;
        workerProfile.Currency = request.Currency ?? workerProfile.Currency;

        workerProfile.UpdatedAt = DateTime.UtcNow;

        // The Angular form sends the whole desired specialization list every
        // time, so replace the stored set rather than diffing it.
        if (request.Specializations is not null)
        {
            var existingSpecs = await dbContext.WorkerSpecializationSpecs
                .Where(s => s.WorkerProfileId == workerProfile.Id)
                .ToListAsync(cancellationToken);

            foreach (var spec in existingSpecs)
                dbContext.WorkerSpecializationSpecs.Remove(spec);

            foreach (var specialization in request.Specializations.Distinct())
            {
                dbContext.WorkerSpecializationSpecs.Add(new WorkerSpecializationSpec
                {
                    WorkerProfileId = workerProfile.Id,
                    WorkerSpecialization = specialization
                });
            }
        }

        if (request.SelfieImageContent is not null && request.SelfieImageFileName is not null)
            await ReplaceDocumentAsync(workerProfile, DocumentType.Selfie, request.SelfieImageContent,
                request.SelfieImageFileName, request.UserId, cancellationToken);

        if (request.PassportImageContent is not null && request.PassportImageFileName is not null)
            await ReplaceDocumentAsync(workerProfile, DocumentType.Passport, request.PassportImageContent,
                request.PassportImageFileName, request.UserId, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task ReplaceDocumentAsync(
        WorkerProfile workerProfile,
        DocumentType type,
        Stream content,
        string fileName,
        string userId,
        CancellationToken cancellationToken)
    {
        var existing = workerProfile.Documents.FirstOrDefault(d => d.Type == type);
        var previousUrl = existing?.DocumentImageUrl;

        var newUrl = await fileStorage.SavePublicAsync(content, fileName, "worker-documents", cancellationToken);

        if (existing is not null)
        {
            existing.DocumentImageUrl = newUrl;
            existing.VerificationStatus = VerificationStatus.Pending;
        }
        else
        {
            workerProfile.Documents.Add(new WorkerDocument
            {
                WorkerId = workerProfile.Id,
                Type = type,
                DocumentImageUrl = newUrl,
                VerificationStatus = VerificationStatus.Pending
            });
        }

        if (!string.IsNullOrEmpty(previousUrl))
            fileStorage.DeletePublic(previousUrl);
    }
}
