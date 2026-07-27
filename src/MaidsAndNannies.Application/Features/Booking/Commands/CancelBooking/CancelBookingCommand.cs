using MediatR;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.CancelBooking;

public sealed record CancelBookingCommand(int BookingId, string UserId) : IRequest<Unit>;