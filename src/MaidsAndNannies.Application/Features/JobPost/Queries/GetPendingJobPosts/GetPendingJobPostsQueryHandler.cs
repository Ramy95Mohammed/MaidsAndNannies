using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.JobPosts.Queries.GetPendingJobPosts;

public sealed class GetPendingJobPostsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetPendingJobPostsQuery, IReadOnlyList<PendingJobPostDto>>
{
    public async Task<IReadOnlyList<PendingJobPostDto>> Handle(GetPendingJobPostsQuery r, CancellationToken ct)
    {
        return await dbContext.JobPosts
            .Where(j => j.PostStatus == JobPostStatus.Pending)
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new PendingJobPostDto(
                j.Id, j.Homeowner.FullName, j.Description,
                j.MonthlySalary, j.DailySalary, j.HourlySalary,
                (int)j.BookingType, (int)j.CommissionType, (int)j.Specialization,
                j.StartDate, j.Quantity, j.CreatedAt , j.Currency.Code))
            .ToListAsync(ct);
    }
}