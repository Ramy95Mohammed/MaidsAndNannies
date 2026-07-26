using MaidsAndNannies.Domain.Enums;
using MediatR;

namespace MaidsAndNannies.Application.Features.JobPosts.Commands.ReviewJobPost;

public sealed record ReviewJobPostCommand(
    int PostId, string SanitizedDescription, bool IsApproved, string? RejectionReason) : IRequest<Unit>;