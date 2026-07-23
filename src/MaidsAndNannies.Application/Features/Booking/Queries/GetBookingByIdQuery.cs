using MediatR;
using MaidsAndNannies.Application.Features.Bookings.Common;

namespace MaidsAndNannies.Application.Features.Bookings.Queries.GetBookingById;

public sealed record GetBookingByIdQuery(int BookingId, string UserId, string Role) : IRequest<BookingDetailDto>;