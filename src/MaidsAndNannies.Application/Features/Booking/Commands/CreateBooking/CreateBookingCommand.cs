using MaidsPlatform.API.Domain.Enums;
using MediatR;

public sealed record CreateBookingCommand(
    string HomeownerId,
    int WorkerId,
    Specialization ServiceType,
    DateTime StartDate,
    decimal MonthlySalary,
    CommissionType CommissionType) : IRequest<int>;