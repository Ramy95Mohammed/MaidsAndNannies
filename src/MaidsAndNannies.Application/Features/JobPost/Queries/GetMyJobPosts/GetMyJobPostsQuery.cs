using MaidsAndNannies.Application.Features.JobPosts.Common;
using MediatR;

namespace MaidsAndNannies.Application.Features.JobPosts.Queries.GetMyJobPosts;

public sealed record GetMyJobPostsQuery(string HomeownerId) : IRequest<IReadOnlyList<JobPostListDto>>;