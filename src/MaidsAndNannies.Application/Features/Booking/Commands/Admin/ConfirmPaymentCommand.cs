using MediatR;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.Admin;

public sealed record ConfirmPaymentCommand(int BookingId, string AdminId) : IRequest<Unit>;