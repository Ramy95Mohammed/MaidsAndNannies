using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Admin.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Admin.Queries.GetAllWorkers;

public sealed class GetAllWorkersQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetAllWorkersQuery, IReadOnlyList<AdminWorkerDto>>
{
    public async Task<IReadOnlyList<AdminWorkerDto>> Handle(GetAllWorkersQuery request, CancellationToken ct)
    {
        return await dbContext.WorkerProfiles
            .Include(w => w.User)
            .Include(w => w.Nationality)
            .Include(w => w.WorkerSpecializationSpecs)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new AdminWorkerDto(
                w.Id,
                w.UserId,
                w.User.FullName,
                w.Nationality != null ? w.Nationality.Name_ar : null,
                w.Nationality != null ? w.Nationality.Name_en : null,
                w.WorkerSpecializationSpecs.Select(s => s.WorkerSpecialization).ToList(),
                w.PassportNumber,
                w.IsAvailable,
                w.VerificationStatus,
                w.CreatedAt))
            .ToListAsync(ct);
    }
}