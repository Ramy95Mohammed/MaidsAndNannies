using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.JobPosts.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.JobPosts.Queries.GetJobApplications;

public sealed class GetJobApplicationsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetJobApplicationsQuery, IReadOnlyList<ApplicationDto>>
{
    public async Task<IReadOnlyList<ApplicationDto>> Handle(GetJobApplicationsQuery r, CancellationToken ct)
    {
        var isOwner = await dbContext.JobPosts.AnyAsync(j => j.Id == r.PostId && j.HomeownerId == r.HomeownerId, ct);
        if (!isOwner) throw new UnauthorizedAccessException();

        return await dbContext.JobApplications
            .Where(a => a.JobPostId == r.PostId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new ApplicationDto(
                a.Id, a.WorkerId, a.Worker.FullName,
                a.Worker.WorkerProfile!.Nationality != null
                    ? a.Worker.WorkerProfile.Nationality.Name_ar
                    : "",
                a.Worker.WorkerProfile.AverageRating, a.Worker.WorkerProfile.TotalReviews,
                a.Message, a.Status, a.CreatedAt))
            .ToListAsync(ct);
    }
}