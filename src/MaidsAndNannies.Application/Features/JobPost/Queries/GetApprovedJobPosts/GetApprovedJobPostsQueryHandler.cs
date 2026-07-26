using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.JobPosts.Common;
using MaidsAndNannies.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.JobPosts.Queries.GetApprovedJobPosts;

public sealed class GetApprovedJobPostsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetApprovedJobPostsQuery, IReadOnlyList<JobPostListDto>>
{
    public async Task<IReadOnlyList<JobPostListDto>> Handle(GetApprovedJobPostsQuery r, CancellationToken ct)
    {
        return await dbContext.JobPosts
            .Where(j => j.PostStatus == JobPostStatus.Approved && j.SanitizedDescription != null)
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new JobPostListDto(
                j.Id, j.SanitizedDescription!, j.MonthlySalary, j.DailySalary, j.HourlySalary,
                j.Specialization, j.BookingType, j.CommissionType, j.StartDate, j.Quantity,
                j.PostStatus, j.RejectionReason, j.CreatedAt, j.Applications.Count , j.Currency.Code))
            .ToListAsync(ct);
    }
}