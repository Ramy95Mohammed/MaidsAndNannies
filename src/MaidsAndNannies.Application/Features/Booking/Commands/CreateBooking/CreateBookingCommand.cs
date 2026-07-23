using MaidsAndNannies.Application.Features.Bookings.Common;
using MaidsPlatform.API.Domain.Enums;
using MediatR;

namespace MaidsAndNannies.Application.Features.Bookings.Commands.CreateBooking;

public sealed record CreateBookingCommand(
    string HomeownerId,
    int WorkerId,
    Specialization ServiceType,
    DateTime StartDate,
    decimal MonthlySalary) : IRequest<int>;