using MaidsAndNannies.Domain.Enums;
using MediatR;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.RequestReplacement;

public sealed record RequestReplacementCommand(
    int BookingId,
    string HomeownerId,
    int NewWorkerId,
    int? ApplicationId,
    ReplacementReason Reason) : IRequest<Unit>;
