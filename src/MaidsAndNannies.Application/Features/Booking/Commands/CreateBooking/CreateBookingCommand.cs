using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;

public sealed record CreateBookingCommand(
    string HomeownerId,
    int WorkerId,
    Specialization ServiceType,
    BookingType BookingType,
    int Quantity,
    DateTime StartDate,
    decimal MonthlySalary,
    decimal DailySalary,
    decimal HourlySalary,
    CommissionType CommissionType) : IRequest<int>;