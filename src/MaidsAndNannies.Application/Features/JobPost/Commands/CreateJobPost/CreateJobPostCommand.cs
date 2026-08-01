using MaidsAndNannies.Domain.Enums;
using MaidsPlatform.API.Domain.Enums;
using MediatR;

namespace MaidsAndNannies.Application.Features.JobPosts.Commands.CreateJobPost;
public sealed record CreateJobPostCommand(
    string HomeownerId, string Description, decimal MonthlySalary,
    decimal DailySalary, decimal HourlySalary, Specialization Specialization,
    BookingType BookingType, CommissionType CommissionType,
    DateTime StartDate, int Quantity, int CurrencyId,
    List<Specialization>? Specializations = null) : IRequest<int>;