using MediatR;
using MaidsAndNannies.Application.Features.Bookings.Common;

namespace MaidsAndNannies.Application.Features.Bookings.Queries.GetMyBookings;

public sealed record GetMyBookingsQuery(string UserId) : IRequest<IReadOnlyList<BookingListDto>>;