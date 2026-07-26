using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.JobPosts.Queries.GetMyJobApplications;

public sealed class GetMyJobApplicationsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetMyJobApplicationsQuery, IReadOnlyList<MyApplicationDto>>
{
    public async Task<IReadOnlyList<MyApplicationDto>> Handle(GetMyJobApplicationsQuery r, CancellationToken ct)
    {
        return await dbContext.JobApplications
            .Where(a => a.WorkerId == r.WorkerId)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new MyApplicationDto(
                a.Id, a.JobPostId, a.Message, a.Status, a.CreatedAt,
                a.JobPost.MonthlySalary, a.JobPost.BookingType, a.JobPost.PostStatus))
            .ToListAsync(ct);
    }
}