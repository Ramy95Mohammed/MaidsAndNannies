using MaidsAndNannies.Application.Common.Exceptions;
using MaidsAndNannies.Application.Common.Helpers;
using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Homeowner.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Homeowner.Queries.GetMyProfile;

public sealed class GetMyHomeownerProfileQueryHandler(
    IApplicationDbContext dbContext,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<GetMyHomeownerProfileQuery, HomeownerProfileDto>
{
    public async Task<HomeownerProfileDto> Handle(GetMyHomeownerProfileQuery request, CancellationToken cancellationToken)
    {
        var profile = await dbContext.HomeownerProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (profile is null)
            throw new NotFoundException("HomeownerProfile", request.UserId);

        return new HomeownerProfileDto(
            profile.Id,
            profile.UserId,
            profile.User.FullName,
            profile.User.Email,
            profile.User.PhoneNumber,
            profile.WhatsAppNumber,
            profile.NationalIdNumber,
            ToAbsolute(profile.NationalIdImage),
            ToAbsolute(profile.SelfieImage),
            ToAbsolute(profile.ProofOfAddressImage),
            profile.Address,
            profile.State,
            profile.City,
            profile.District,
            profile.VerificationStatus,
            profile.VerificationNotes,
            profile.VerifiedAt);
    }

    private string? ToAbsolute(string? relativeUrl) =>
        AbsoluteUrlHelper.ToAbsoluteUrl(relativeUrl, httpContextAccessor);
}