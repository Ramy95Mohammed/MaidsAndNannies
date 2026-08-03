using MaidsAndNannies.Application.Features.JobPosts.Common;
using MaidsAndNannies.Application.Features.Worker.Common;
using MediatR;

namespace MaidsAndNannies.Application.Features.JobPosts.Queries.GetApprovedJobPosts;

public sealed record GetApprovedJobPostsQuery(
    int? Specialization = null,
    int? BookingType = null,
    decimal? MinMonthlySalary = null,
    decimal? MaxMonthlySalary = null,
    int Page = 1,
    int PageSize = 12) : IRequest<PagedResult<JobPostListDto>>;