using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.JobPosts.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.JobPosts.Queries.GetMyJobPosts;

public sealed class GetMyJobPostsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetMyJobPostsQuery, IReadOnlyList<JobPostListDto>>
{
    public async Task<IReadOnlyList<JobPostListDto>> Handle(GetMyJobPostsQuery r, CancellationToken ct)
    {
        return await dbContext.JobPosts
            .Where(j => j.HomeownerId == r.HomeownerId)
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new JobPostListDto(
                j.Id, j.Description, j.MonthlySalary, j.DailySalary, j.HourlySalary,
    j.Specialization, j.BookingType, j.CommissionType, j.StartDate, j.Quantity,
    j.PostStatus, j.RejectionReason, j.CreatedAt, j.Applications.Count, j.Currency.Code , j.Specializations.Select(s => s.JobSpecialization).ToList()))
            .ToListAsync(ct);
    }
}