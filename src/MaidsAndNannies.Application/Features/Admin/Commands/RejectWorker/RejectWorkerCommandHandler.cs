using MaidsAndNannies.Application.Common.Exceptions;
using MaidsAndNannies.Application.Common.Interfaces;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Admin.Commands.RejectWorker;

public sealed class RejectWorkerCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<RejectWorkerCommand, Unit>
{
    public async Task<Unit> Handle(RejectWorkerCommand request, CancellationToken cancellationToken)
    {
        var worker = await dbContext.WorkerProfiles
            .FirstOrDefaultAsync(w => w.Id == request.WorkerProfileId, cancellationToken);

        if (worker is null)
            throw new NotFoundException("WorkerProfile", request.WorkerProfileId);

        worker.VerificationStatus = VerificationStatus.Rejected;
        worker.VerificationNotes = request.Reason;
        worker.VerifiedAt = DateTime.UtcNow;
        worker.VerifiedBy = request.AdminUserId;
        worker.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
