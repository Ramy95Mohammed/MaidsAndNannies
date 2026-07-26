using MaidsAndNannies.Application.Features.JobPosts.Common;
using MediatR;

namespace MaidsAndNannies.Application.Features.JobPosts.Queries.GetJobApplications;

public sealed record GetJobApplicationsQuery(int PostId, string HomeownerId) : IRequest<IReadOnlyList<ApplicationDto>>;