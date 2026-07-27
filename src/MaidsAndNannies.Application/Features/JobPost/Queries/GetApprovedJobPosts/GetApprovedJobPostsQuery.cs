using MaidsAndNannies.Application.Features.JobPosts.Common;
using MediatR;

namespace MaidsAndNannies.Application.Features.JobPosts.Queries.GetApprovedJobPosts;

public sealed record GetApprovedJobPostsQuery : IRequest<IReadOnlyList<JobPostListDto>>;