using MaidsAndNannies.Application.Common.Exceptions;
using MaidsAndNannies.Application.Common.Interfaces;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Admin.Commands.ApproveHomeowner;

public sealed class ApproveHomeownerCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<ApproveHomeownerCommand, Unit>
{
    public async Task<Unit> Handle(ApproveHomeownerCommand request, CancellationToken cancellationToken)
    {
        var homeowner = await dbContext.HomeownerProfiles
            .FirstOrDefaultAsync(h => h.Id == request.HomeownerProfileId, cancellationToken);

        if (homeowner is null)
            throw new NotFoundException("HomeownerProfile", request.HomeownerProfileId);

        homeowner.VerificationStatus = VerificationStatus.Approved;
        homeowner.VerificationNotes = null;
        homeowner.VerifiedAt = DateTime.UtcNow;
        homeowner.VerifiedBy = request.AdminUserId;
        homeowner.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
