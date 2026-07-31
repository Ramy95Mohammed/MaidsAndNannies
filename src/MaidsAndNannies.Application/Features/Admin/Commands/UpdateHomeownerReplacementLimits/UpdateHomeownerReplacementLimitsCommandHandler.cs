using MaidsAndNannies.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Admin.Commands.UpdateHomeownerReplacementLimits;

public sealed class UpdateHomeownerReplacementLimitsCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<UpdateHomeownerReplacementLimitsCommand, Unit>
{
    public async Task<Unit> Handle(UpdateHomeownerReplacementLimitsCommand request, CancellationToken ct)
    {
        var profile = await dbContext.HomeownerProfiles.FindAsync([request.HomeownerProfileId], ct)
            ?? throw new KeyNotFoundException("صاحبة المنزل غير موجودة");

        profile.MaxFaultReplacementCount = request.MaxFaultReplacementCount;
        profile.MaxPreferenceReplacementCount = request.MaxPreferenceReplacementCount;
        profile.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}