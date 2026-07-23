using MediatR;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.Admin;

public sealed record ConfirmWorkerCommand(int BookingId, string AdminId) : IRequest<Unit>;