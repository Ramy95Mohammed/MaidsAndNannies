using MaidsAndNannies.Application.Common.Exceptions;
using MaidsAndNannies.Application.Common.Helpers;
using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Worker.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Worker.Queries.GetMyProfile;

public sealed class GetMyWorkerProfileQueryHandler(
    IApplicationDbContext dbContext,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<GetMyWorkerProfileQuery, WorkerProfileDto>
{
    public async Task<WorkerProfileDto> Handle(GetMyWorkerProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await dbContext.WorkerProfiles
            .Include(p => p.User)
            .Include(p => p.Documents)
            .Include(p => p.WorkerSpecializationSpecs)
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (profile is null)
            throw new NotFoundException("WorkerProfile", request.UserId);

        return new WorkerProfileDto(
            profile.Id,
            profile.UserId,
            profile.User.FullName,
            profile.User.Email,
            profile.User.PhoneNumber,
            profile.WhatsAppNumber,
            profile.NationalityId,
            profile.NationalIdNumber,
            profile.BirthDate,
            profile.PassportNumber,
            profile.PassportExpiryDate,
            profile.PassportCountry,
            profile.CountryId,
            profile.StateId,
            profile.CityId,
            profile.Address,
            profile.Bio,
            profile.ExperienceYears,
            profile.PreviousEmployer,
            (profile.WorkerSpecializationSpecs ?? [])
                .Select(s => new WorkerSpecializationDto(s.Id, s.WorkerSpecialization)).ToList(),
            profile.IsLiveIn,
            profile.IsAvailable,
            profile.MonthlyRate,
            profile.HourlyRate,
            profile.Currency,
            profile.AverageRating,
            profile.TotalReviews,
            profile.Languages,
            profile.VerificationStatus,
            profile.VerificationNotes,
            profile.VerifiedAt,
            profile.VerifiedBy,
            profile.Documents
                .Select(d => new WorkerDocumentDto(
                    d.Type,
                    AbsoluteUrlHelper.ToAbsoluteUrl(d.DocumentImageUrl, httpContextAccessor),
                    d.VerificationStatus))
                .ToList());
    }
}
