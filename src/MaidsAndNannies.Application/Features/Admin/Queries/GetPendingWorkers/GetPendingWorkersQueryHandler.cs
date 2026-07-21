using MaidsAndNannies.Application.Common.Helpers;
using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.Admin.Common;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.Admin.Queries.GetPendingWorkers;

public sealed class GetPendingWorkersQueryHandler(
    IApplicationDbContext dbContext,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<GetPendingWorkersQuery, IReadOnlyList<PendingWorkerDto>>
{
    public async Task<IReadOnlyList<PendingWorkerDto>> Handle(GetPendingWorkersQuery request, CancellationToken cancellationToken)
    {
        var workers = await dbContext.WorkerProfiles
            .Include(w => w.User)
            .Include(w => w.Documents)
            .Where(w => w.VerificationStatus == VerificationStatus.Pending)
            .OrderBy(w => w.CreatedAt)
            .ToListAsync(cancellationToken);

        return workers.Select(w => new PendingWorkerDto(
            w.Id,
            w.UserId,
            w.User.FullName,
            w.User.Email,
            w.User.PhoneNumber,
            w.NationalityId,
            w.NationalIdNumber,
            w.PassportNumber,
            w.ExperienceYears,
            w.VerificationStatus,
            w.CreatedAt,
            w.Documents.Select(d => new PendingDocumentDto(
                d.Type.ToString(),
                AbsoluteUrlHelper.ToAbsoluteUrl(d.DocumentImageUrl, httpContextAccessor))).ToList()
        )).ToList();
    }
}
