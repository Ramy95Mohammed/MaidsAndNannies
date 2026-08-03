using MaidsAndNannies.Application.Features.Bookings.Common;
using MaidsAndNannies.Application.Features.Worker.Common;
using MediatR;

namespace MaidsAndNannies.Application.Features.Bookings.Queries.GetMyBookings;

public sealed record GetMyBookingsQuery(
    string UserId,
    string Role,
    int? Status = null,
    int? BookingType = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 10) : IRequest<PagedResult<BookingListDto>>;