using MediatR;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.RequestReplacement;

public sealed record RequestReplacementCommand(
    int BookingId,
    string HomeownerId,
    int NewWorkerId) : IRequest<Unit>;