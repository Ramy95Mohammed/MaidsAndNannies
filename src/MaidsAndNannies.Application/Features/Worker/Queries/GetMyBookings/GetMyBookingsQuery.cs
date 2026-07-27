using MaidsAndNannies.Application.Features.Worker.Common;
using MediatR;

namespace MaidsAndNannies.Application.Features.Worker.Queries.GetMyBookings;

public sealed record GetMyBookingsQuery(string UserId) : IRequest<IReadOnlyList<MyBookingDto>>;
