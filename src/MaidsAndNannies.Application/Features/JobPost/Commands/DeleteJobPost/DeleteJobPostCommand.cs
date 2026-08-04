using MediatR;

namespace MaidsAndNannies.Application.Features.JobPosts.Commands.DeleteJobPost;

public sealed record DeleteJobPostCommand(int PostId, string HomeownerId) : IRequest<Unit>;