using MediatR;

namespace MaidsAndNannies.Application.Features.JobPosts.Commands.ApplyForJob;

public sealed record ApplyForJobCommand(int JobPostId, string WorkerId, string? Message) : IRequest<Unit>;