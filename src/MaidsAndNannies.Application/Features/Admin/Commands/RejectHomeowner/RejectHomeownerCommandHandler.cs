using MaidsAndNannies.Application.Common.Exceptions;
using MaidsAndNannies.Application.Common.Interfaces;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Admin.Commands.RejectHomeowner;

public sealed class RejectHomeownerCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<RejectHomeownerCommand, Unit>
{
    public async Task<Unit> Handle(RejectHomeownerCommand request, CancellationToken cancellationToken)
    {
        var homeowner = await dbContext.HomeownerProfiles
            .FirstOrDefaultAsync(h => h.Id == request.HomeownerProfileId, cancellationToken);

        if (homeowner is null)
            throw new NotFoundException("HomeownerProfile", request.HomeownerProfileId);

        homeowner.VerificationStatus = VerificationStatus.Rejected;
        homeowner.VerificationNotes = request.Reason;
        homeowner.VerifiedAt = DateTime.UtcNow;
        homeowner.VerifiedBy = request.AdminUserId;
        homeowner.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
