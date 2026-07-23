using MediatR;
using MaidsAndNannies.Application.Features.Bookings.Common;

namespace MaidsAndNannies.Application.Features.Admin.Queries.GetAllBookings;

public sealed record GetAllBookingsQuery : IRequest<IReadOnlyList<AdminBookingListDto>>;