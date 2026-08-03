using MaidsAndNannies.Application.Common.Interfaces;
using MaidsAndNannies.Application.Features.JobPosts.Common;
using MaidsAndNannies.Application.Features.Worker.Common;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MaidsAndNannies.Application.Features.JobPosts.Queries.GetApprovedJobPosts;

public sealed class GetApprovedJobPostsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetApprovedJobPostsQuery, PagedResult<JobPostListDto>>
{
    public async Task<PagedResult<JobPostListDto>> Handle(GetApprovedJobPostsQuery r, CancellationToken ct)
    {
        if (r.Page < 1) r = r with { Page = 1 };
        if (r.PageSize < 1 || r.PageSize > 50) r = r with { PageSize = 12 };

        var query = dbContext.JobPosts
            .Where(j => j.PostStatus == JobPostStatus.Approved && j.SanitizedDescription != null);

        if (r.Specialization.HasValue)
            query = query.Where(j => j.Specialization == (Specialization)r.Specialization.Value);
        if (r.BookingType.HasValue)
            query = query.Where(j => j.BookingType == (BookingType)r.BookingType.Value);
        if (r.MinMonthlySalary.HasValue)
            query = query.Where(j => j.MonthlySalary >= r.MinMonthlySalary.Value);
        if (r.MaxMonthlySalary.HasValue)
            query = query.Where(j => j.MonthlySalary <= r.MaxMonthlySalary.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(j => j.CreatedAt)
            .Select(j => new JobPostListDto(
                j.Id, j.SanitizedDescription!, j.MonthlySalary, j.DailySalary, j.HourlySalary,
                j.Specialization, j.BookingType, j.CommissionType, j.StartDate, j.Quantity,
                j.PostStatus, j.RejectionReason, j.CreatedAt, j.Applications.Count, j.Currency.Code,
                j.Specializations.Select(s => s.JobSpecialization).ToList()))
            .Skip((r.Page - 1) * r.PageSize)
            .Take(r.PageSize)
            .ToListAsync(ct);

        return new PagedResult<JobPostListDto>(items, totalCount, r.Page, r.PageSize);
    }
}