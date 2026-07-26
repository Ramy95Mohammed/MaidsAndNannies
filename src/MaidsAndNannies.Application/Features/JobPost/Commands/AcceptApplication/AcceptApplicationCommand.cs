using MediatR;

namespace MaidsAndNannies.Application.Features.JobPosts.Commands.AcceptApplication;

public sealed record AcceptApplicationCommand(int PostId, int AppId, string HomeownerId) : IRequest<int>; // returns BookingId