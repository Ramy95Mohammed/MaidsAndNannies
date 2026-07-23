using MediatR;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.Admin;

public sealed record StartWorkCommand(int BookingId, string AdminId) : IRequest<Unit>;