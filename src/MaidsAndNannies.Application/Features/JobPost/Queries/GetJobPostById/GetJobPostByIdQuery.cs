using MaidsAndNannies.Application.Features.JobPosts.Common;
using MediatR;

namespace MaidsAndNannies.Application.Features.JobPosts.Queries.GetJobPostById;

public sealed record GetJobPostByIdQuery(int Id, string UserId, string Role) : IRequest<JobPostDetailDto>;