using MediatR;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.Admin;

public sealed record CompleteWorkCommand(int BookingId, string AdminId) : IRequest<Unit>;