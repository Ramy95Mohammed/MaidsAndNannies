using MaidsAndNannies.Application.Features.Bookings.Common;
using MaidsAndNannies.Application.Features.Worker.Common;
using MediatR;

namespace MaidsAndNannies.Application.Features.Admin.Queries.GetAllBookings;

public sealed record GetAllBookingsQuery(
    int? Status = null,
    bool? IsPaid = null,
    string? Search = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 10) : IRequest<PagedResult<AdminBookingListDto>>;