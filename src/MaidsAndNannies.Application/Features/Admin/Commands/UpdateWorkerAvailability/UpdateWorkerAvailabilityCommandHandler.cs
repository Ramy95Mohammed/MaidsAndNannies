using MaidsAndNannies.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Admin.Commands.UpdateWorkerAvailability;

public sealed class UpdateWorkerAvailabilityCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<UpdateWorkerAvailabilityCommand, Unit>
{
    public async Task<Unit> Handle(UpdateWorkerAvailabilityCommand r, CancellationToken ct)
    {
        var worker = await dbContext.WorkerProfiles
            .FirstOrDefaultAsync(w => w.Id == r.WorkerId, ct)
            ?? throw new KeyNotFoundException("العاملة غير موجودة");

        worker.IsAvailable = r.IsAvailable;
        worker.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(ct);
        return Unit.Value;
    }
}