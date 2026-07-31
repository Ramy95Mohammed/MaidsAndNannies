using MaidsAndNannies.Application.Features.Bookings.Common;
using MaidsAndNannies.Domain.Entities;
using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaidsAndNannies.Application.Features.Booking.Queries
{
    public sealed record GetBookingCreationInfoQuery(
    string HomeownerId,
    int WorkerId,
    Specialization ServiceType,
    BookingType BookingType,
    int Quantity,
    DateTime StartDate,
    decimal MonthlySalary,
    decimal DailySalary,
    decimal HourlySalary,
    CommissionType CommissionType) : IRequest<BookingDetailDto>;    
}
