using MediatR;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.Admin;

public sealed record RequestPaymentCommand(int BookingId, string AdminId) : IRequest<Unit>;