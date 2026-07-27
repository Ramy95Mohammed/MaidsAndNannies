using MediatR;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.RequestReplacement;

public sealed record RequestReplacementCommand(
    int BookingId,
    string HomeownerId,
    int NewWorkerId , int? ApplicationId) : IRequest<Unit>;